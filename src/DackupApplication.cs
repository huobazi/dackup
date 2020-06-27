using System;
using System.IO;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using dackup.Configuration;

namespace dackup
{
    public class DackupApplication
    {
        private readonly ILogger logger;
        public DackupApplication(ILogger<DackupApplication> logger)
        {
            this.logger = logger;
        }
        public async Task Run(string configFilePath)
        {      
            var statistics           = new Statistics();
                statistics.StartedAt = DateTime.Now;
            var performConfig        = PrepaireConfig(configFilePath);
            if(performConfig == null)
            {
                return;
            }
            statistics.ModelName = performConfig.Name;
            Directory.CreateDirectory(DackupContext.Current.TmpPath);

            // run backup
            var backupTasks = RunBackup(performConfig);
            await backupTasks;

            // run store
            var (storageUploadTasks, storagePurgeTasks) = RunStorage(performConfig);     
            await storageUploadTasks;
            statistics.FinishedAt = DateTime.Now;

            // run notify                     
            var notifyTasks = RunNotify(performConfig, statistics);

            Clean();
            
            await notifyTasks;
            await storagePurgeTasks;

            logger.LogInformation("Dackup done ");
        }
        private PerformConfig PrepaireConfig(string configfile)
        {
            FileStream fs = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PerformConfig));
                              fs         = new FileStream(configfile, FileMode.Open, FileAccess.Read);
                return (PerformConfig)serializer.Deserialize(fs);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "[exception]: on DackupApplication.PrepaireConfig");
                return null;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }
        private Task<BackupTaskResult[]> RunBackup(PerformConfig cfg)
        {
            logger.LogInformation("Dackup start backup task ");

            var backupTaskList   = ParseBackupTaskFromConfig(cfg);
            var backupTaskResult = new List<Task<BackupTaskResult>>();

            backupTaskList.ForEach(task =>
            {
                backupTaskResult.Add(task.BackupAsync());
            });

            return Task.WhenAll(backupTaskResult.ToArray());
        }
        private (Task<UploadResult[]>, Task<PurgeResult[]>) RunStorage(PerformConfig cfg)
        {
            logger.LogInformation("Dackup start storage task ");

            var storageList = ParseStorageFromConfig(cfg);
            var storageUploadResultList = new List<Task<UploadResult>>();
            var storagePurgeResultList = new List<Task<PurgeResult>>();

            storageList.ForEach(storage =>
            {
                DackupContext.Current.GenerateFilesList.ForEach(file =>
                {
                    storageUploadResultList.Add(storage.UploadAsync(file));
                });
                storagePurgeResultList.Add(storage.PurgeAsync());
            });

            var storageUploadTasks = Task.WhenAll(storageUploadResultList.ToArray());
            var storagePurgeTasks  = Task.WhenAll(storagePurgeResultList.ToArray());

            return (storageUploadTasks, storagePurgeTasks);
        }
        private Task<NotifyResult[]> RunNotify(PerformConfig cfg, Statistics statistics)
        {
            logger.LogInformation("Dackup start notify task ");

            var notifyList       = ParseNotifyFromConfig(cfg);
            var notifyResultList = new List<Task<NotifyResult>>();

            notifyList.ForEach(notify =>
            {
                notifyResultList.Add(notify.NotifyAsync(statistics));
            });

            return Task.WhenAll(notifyResultList.ToArray());
        }
        private void Clean()
        {
            logger.LogInformation("Dackup clean tmp folder ");

            var di = new DirectoryInfo(DackupContext.Current.TmpPath);
            foreach (var file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (var dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            Directory.Delete(DackupContext.Current.TmpPath);
        }
        private List<IBackupTask> ParseBackupTaskFromConfig(PerformConfig config)
        {
            var tasks = new List<IBackupTask>();
            if (config != null)
            {
                if (config.Archives != null && config.Archives.Count > 0)
                {
                    config.Archives.ForEach(cfg =>
                    {
                        ArchiveBackupTask task = PopulateArchiveBackupTask(cfg);
                        tasks.Add(task);
                    });
                }
                if (config.Databases != null && config.Databases.Count > 0)
                {
                    config.Databases.ForEach(dbConfig =>
                    {
                        if (dbConfig.Enable)
                        {
                            if (dbConfig.Type.ToLower().Trim() == "postgres")
                            {
                                PostgresBackupTask task = PopulatePostgresBackupTask(dbConfig);
                                tasks.Add(task);
                            }
                            else if (dbConfig.Type.ToLower().Trim() == "mysql")
                            {
                                MySqlBackupTask task = PopulateMysqlBuckupTask(dbConfig);
                                tasks.Add(task);
                            }
                            else if (dbConfig.Type.ToLower().Trim() == "mongodb")
                            {
                                MongoDBBackupTask task = PopulateMongoDBBackupTask(dbConfig);
                                tasks.Add(task);
                            }
                            else if (dbConfig.Type.ToLower().Trim() == "mssql")
                            {
                                MsSqlBackupTask task = PopulateMsSqlBackupTask(dbConfig);
                                tasks.Add(task);
                            }
                        }
                    });
                }
            }

            return tasks;
        }
        private List<IStorage> ParseStorageFromConfig(PerformConfig config)
        {
            var tasks = new List<IStorage>();
            if (config != null)
            {
                if (config.Storages != null && config.Storages != null)
                {
                    config.Storages.ForEach(storageConfig =>
                    {
                        if (storageConfig.Enable)
                        {
                            if (storageConfig.Type.ToLower().Trim() == "local")
                            {
                                var task = ServiceProviderFactory.ServiceProvider.GetService<LocalStorage>();
                                storageConfig.OptionList.NullSafeSetTo("path", s => task.Path = s);
                                if (storageConfig.OptionList.Find(c => c.Name.ToLower() == "remove_threshold") != null)
                                {
                                    task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(storageConfig.OptionList.Find(c => c.Name.ToLower() == "remove_threshold").Value);
                                }
                                tasks.Add(task);
                            }
                            if (storageConfig.Type.ToLower().Trim() == "s3")
                            {
                                S3Storage task = PopulateS3Storage(storageConfig);
                                tasks.Add(task);
                            }
                            if (storageConfig.Type.ToLower().Trim() == "aliyun_oss")
                            {
                                AliyunOssStorage task = PopulateAliyunOssStorage(storageConfig);
                                tasks.Add(task);
                            }
                        }
                    });
                }
            }
            
            return tasks;
        }
        private List<INotify> ParseNotifyFromConfig(PerformConfig config)
        {
            var tasks = new List<INotify>();
            if (config != null)
            {
                if (config.Notifiers != null && config.Notifiers.HttpPostList != null)
                {
                    config.Notifiers.HttpPostList.ForEach(cfg =>
                    {
                        if (cfg.Enable)
                        {
                            HttpPostNotify httpPost = PopulateHttpPostNotify(cfg);
                            tasks.Add(httpPost);
                        }
                    });

                }
                if (config.Notifiers != null && config.Notifiers.DingtalkRobotList != null)
                {
                    config.Notifiers.DingtalkRobotList.ForEach(cfg =>
                    {
                        if (cfg.Enable)
                        {
                            DingtalkRobotNotify dingtalkRobot = PopulateDingtalkRobotNotify(cfg);
                            tasks.Add(dingtalkRobot);
                        }
                    });

                }
                if (config.Notifiers != null && config.Notifiers.SlackList != null)
                {
                    config.Notifiers.SlackList.ForEach(cfg =>
                    {
                        if (cfg.Enable)
                        {
                            SlackNotify slack = PopulateSlackNotify(cfg);
                            tasks.Add(slack);
                        }
                    });
                }
                if (config.Notifiers != null && config.Notifiers.EmailList != null)
                {
                    config.Notifiers.EmailList.ForEach(cfg =>
                    {
                        if (cfg.Enable)
                        {
                            var deliveryMethod = cfg.DeliveryMethod;
                            if (!string.IsNullOrWhiteSpace(deliveryMethod) && deliveryMethod.Trim().ToLower() == "smtp")
                            {
                                SmtpEmailNotify email = PopulateEmailSmtpNotify(cfg);
                                tasks.Add(email);
                            }
                        }
                    });
                }
            }

            return tasks;
        }
        private AliyunOssStorage PopulateAliyunOssStorage(Storage storageConfig)
        {            
            var task            = ServiceProviderFactory.ServiceProvider.GetService<AliyunOssStorage>();

            var endpoint        = storageConfig.OptionList?.Find(c => c.Name.ToLower() == "endpoint")?.Value;
            var accessKeyId     = storageConfig.OptionList?.Find(c => c.Name.ToLower() == "access_key_id")?.Value;
            var accessKeySecret = storageConfig.OptionList?.Find(c => c.Name.ToLower() == "secret_access_key")?.Value;
            var bucketName          = storageConfig.OptionList?.Find(c => c.Name.ToLower() == "bucket")?.Value;
            
            storageConfig.OptionList.NullSafeSetTo("endpoint", s => task.Endpoint = s);
            storageConfig.OptionList.NullSafeSetTo("access_key_id", s => task.AccessKeyId = s);
            storageConfig.OptionList.NullSafeSetTo("secret_access_key", s => task.AccessKeySecret = s);
            storageConfig.OptionList.NullSafeSetTo("bucket", s => task.BucketName = s);
            storageConfig.OptionList.NullSafeSetTo("path", s => task.PathPrefix = s);


            if (storageConfig.OptionList?.Find(c => c.Name.ToLower() == "remove_threshold") != null)
            {
                task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(storageConfig.OptionList.Find(c => c.Name.ToLower() == "remove_threshold").Value);
            }

            return task;
        }
        private S3Storage PopulateS3Storage(Storage storageConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<S3Storage>();
            
            storageConfig.OptionList.NullSafeSetTo("path", s => task.PathPrefix = s);
            storageConfig.OptionList.NullSafeSetTo("region", s => task.Region = s);
            storageConfig.OptionList.NullSafeSetTo("access_key_id", s => task.AccessKeyId = s);
            storageConfig.OptionList.NullSafeSetTo("secret_access_key", s => task.AccessKeySecret = s);
            storageConfig.OptionList.NullSafeSetTo("bucket", s => task.BucketName = s);

            if (storageConfig.OptionList?.Find(c => c.Name.ToLower() == "remove_threshold") != null)
            {
                task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(storageConfig.OptionList.Find(c => c.Name.ToLower() == "remove_threshold").Value);
            }

            return task;
        }
        private ArchiveBackupTask PopulateArchiveBackupTask(Archive cfg)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<ArchiveBackupTask>();

            task.Name            = cfg.Name;
            task.IncludePathList = cfg.Includes;
            task.ExcludePathList = cfg.Excludes;

            return task;
        }
        private PostgresBackupTask PopulatePostgresBackupTask(Database dbConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<PostgresBackupTask>();
            dbConfig.OptionList.NullSafeSetTo("host", s => task.Host = s);
            dbConfig.OptionList.NullSafeSetTo("port", s => task.Port = s);
            dbConfig.OptionList.NullSafeSetTo("database", s => task.Database = s);
            dbConfig.OptionList.NullSafeSetTo("username", s => task.UserName = s);
            dbConfig.OptionList.NullSafeSetTo("password", s => task.Password = s);
            dbConfig.OptionList.NullSafeSetTo("path_to_pg_dump", s => task.PathToPgDump = s);
            if (dbConfig.AdditionalOptionList != null && dbConfig.AdditionalOptionList.Count > 0)
            {
                dbConfig.AdditionalOptionList.ForEach(c =>
                {
                    task.AddCommandOptions(c.Name, c.Value);
                });
            }

            return task;
        }
        private MySqlBackupTask PopulateMysqlBuckupTask(Database dbConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<MySqlBackupTask>();
            dbConfig.OptionList.NullSafeSetTo("host", s => task.Host = s);
            dbConfig.OptionList.NullSafeSetTo("port", s => task.Port = s);
            dbConfig.OptionList.NullSafeSetTo("database", s => task.Database = s);
            dbConfig.OptionList.NullSafeSetTo("username", s => task.UserName = s);
            dbConfig.OptionList.NullSafeSetTo("password", s => task.Password = s);
            dbConfig.OptionList.NullSafeSetTo("path_to_mysqldump", s => task.PathToMysqlDump = s);
            if (dbConfig.AdditionalOptionList != null && dbConfig.AdditionalOptionList.Count > 0)
            {
                dbConfig.AdditionalOptionList.ForEach(c =>
                {
                    task.AddCommandOptions(c.Name, c.Value);
                });
            }

            return task;
        }
        private MongoDBBackupTask PopulateMongoDBBackupTask(Database dbConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<MongoDBBackupTask>();
            dbConfig.OptionList.NullSafeSetTo("host", s => task.Host = s);
            dbConfig.OptionList.NullSafeSetTo("port", s => task.Port = s);
            dbConfig.OptionList.NullSafeSetTo("database", s => task.Database = s);
            dbConfig.OptionList.NullSafeSetTo("username", s => task.UserName = s);
            dbConfig.OptionList.NullSafeSetTo("password", s => task.Password = s);
            dbConfig.OptionList.NullSafeSetTo("path_to_mongodump", s => task.PathToMongoDump = s);
            if (dbConfig.AdditionalOptionList != null && dbConfig.AdditionalOptionList.Count > 0)
            {
                dbConfig.AdditionalOptionList.ForEach(c =>
                {
                    task.AddCommandOptions(c.Name, c.Value);
                });
            }

            return task;
        }
        private MsSqlBackupTask PopulateMsSqlBackupTask(Database dbConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<MsSqlBackupTask>();
            dbConfig.OptionList.NullSafeSetTo("host", s => task.Host = s);
            dbConfig.OptionList.NullSafeSetTo("port", s => task.Port = s);
            dbConfig.OptionList.NullSafeSetTo("database", s => task.Database = s);
            dbConfig.OptionList.NullSafeSetTo("username", s => task.UserName = s);
            dbConfig.OptionList.NullSafeSetTo("password", s => task.Password = s);
            dbConfig.OptionList.NullSafeSetTo("path_to_mssqldump", s => task.PathToMssqlDump = s);
            if (dbConfig.AdditionalOptionList != null && dbConfig.AdditionalOptionList.Count > 0)
            {
                dbConfig.AdditionalOptionList.ForEach(c =>
                {
                    task.AddCommandOptions(c.Name, c.Value);
                });
            }

            return task;
        }
        private SmtpEmailNotify PopulateEmailSmtpNotify(Email cfg)
        {
            var emailNotify       = ServiceProviderFactory.ServiceProvider.GetService<SmtpEmailNotify>();
            emailNotify.Enable    = cfg.Enable;
            emailNotify.OnFailure = cfg.OnFailure;
            emailNotify.OnSuccess = cfg.OnSuccess;
            emailNotify.OnWarning = cfg.OnWarning;
            cfg.OptionList.NullSafeSetTo("from", s => emailNotify.From = s);
            cfg.OptionList.NullSafeSetTo("to", s => emailNotify.To = s);
            cfg.OptionList.NullSafeSetTo("address", s => emailNotify.Address = s);
            cfg.OptionList.NullSafeSetTo("domain", s => emailNotify.Domain = s);
            cfg.OptionList.NullSafeSetTo("user_name", s => emailNotify.UserName = s);
            cfg.OptionList.NullSafeSetTo("password", s => emailNotify.Password = s);
            cfg.OptionList.NullSafeSetTo("authentication", s => emailNotify.Authentication = s);
            cfg.OptionList.NullSafeSetTo("enable_starttls", s => emailNotify.EnableStarttls = s);
            cfg.OptionList.NullSafeSetTo("cc", s => emailNotify.CC = s);
            cfg.OptionList.NullSafeSetTo("bcc", s => emailNotify.BCC = s);
            cfg.OptionList.NullSafeSetTo("port", port => emailNotify.Port = port);

            return emailNotify;
        }
        private SlackNotify PopulateSlackNotify(Slack cfg)
        {
            var slackNotify       = ServiceProviderFactory.ServiceProvider.GetService<SlackNotify>();
            slackNotify.Enable    = cfg.Enable;
            slackNotify.OnFailure = cfg.OnFailure;
            slackNotify.OnSuccess = cfg.OnSuccess;
            slackNotify.OnWarning = cfg.OnWarning;
            cfg.OptionList.NullSafeSetTo("webhook_url", s => slackNotify.WebHookUrl = s);
            cfg.OptionList.NullSafeSetTo("channel", s => slackNotify.Channel = s);
            cfg.OptionList.NullSafeSetTo("icon_emoji", s => slackNotify.Icon_emoji = s);
            cfg.OptionList.NullSafeSetTo("username", s => slackNotify.UserName = s);

            return slackNotify;
        }
        private DingtalkRobotNotify PopulateDingtalkRobotNotify(DingtalkRobot cfg)
        {
            var dingtalkRobotNotify       = ServiceProviderFactory.ServiceProvider.GetService<DingtalkRobotNotify>();
            cfg.OptionList.NullSafeSetTo("url", s => dingtalkRobotNotify.WebHookUrl = s);
            dingtalkRobotNotify.AtAll     = cfg.AtAll;
            dingtalkRobotNotify.Enable    = cfg.Enable;
            dingtalkRobotNotify.OnFailure = cfg.OnFailure;
            dingtalkRobotNotify.OnSuccess = cfg.OnSuccess;
            dingtalkRobotNotify.OnWarning = cfg.OnWarning;

            if (cfg.AtMobiles != null && cfg.AtMobiles.Count > 0)
            {
                dingtalkRobotNotify.AtMobiles = new List<string>();
                cfg.AtMobiles.ForEach(header =>
                {
                    dingtalkRobotNotify.AtMobiles.AddRange(header.Value.Split(';', StringSplitOptions.RemoveEmptyEntries));
                });
            }
            dingtalkRobotNotify.AtMobiles = dingtalkRobotNotify.AtMobiles.Distinct().ToList();

            return dingtalkRobotNotify;
        }
        private HttpPostNotify PopulateHttpPostNotify(HttpPost cfg)
        {
            var httpPostNotify       = ServiceProviderFactory.ServiceProvider.GetService<HttpPostNotify>();
            cfg.OptionList.NullSafeSetTo("url", s => httpPostNotify.WebHookUrl = s);
            httpPostNotify.Enable    = cfg.Enable;
            httpPostNotify.OnFailure = cfg.OnFailure;
            httpPostNotify.OnSuccess = cfg.OnSuccess;
            httpPostNotify.OnWarning = cfg.OnWarning;
            if (cfg.Headers != null)
            {
                httpPostNotify.Headers = new NameValueCollection();
                cfg.Headers.ForEach(header =>
                {
                    httpPostNotify.Headers[header.Name] = header.Value;
                });
            }
            var paramsList = cfg.OptionList?.Where(c => c.Name.ToLower() != "url")?.ToList();
            if (paramsList != null)
            {
                httpPostNotify.Params = new NameValueCollection();
                paramsList.ForEach(param =>
                {
                    httpPostNotify.Params[param.Name] = param.Value;
                });
            }

            return httpPostNotify;
        }
    }
}