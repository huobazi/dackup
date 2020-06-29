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
using dackup.Extensions;

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
            if (performConfig == null)
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
                fs = new FileStream(configfile, FileMode.Open, FileAccess.Read);
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

            var storageList             = ParseStorageFromConfig(cfg);
            var storageUploadResultList = new List<Task<UploadResult>>();
            var storagePurgeResultList  = new List<Task<PurgeResult>>();

            storageList.ForEach(storage =>
            {
                DackupContext.Current.GenerateFilesList.ForEach(file =>
                {
                    storageUploadResultList.Add(storage.UploadAsync(file));
                });
                storagePurgeResultList.Add(storage.PurgeAsync());
            });

            var storageUploadTasks = Task.WhenAll(storageUploadResultList.ToArray());
            var storagePurgeTasks = Task.WhenAll(storagePurgeResultList.ToArray());

            return (storageUploadTasks, storagePurgeTasks);
        }
        private Task<NotifyResult[]> RunNotify(PerformConfig cfg, Statistics statistics)
        {
            logger.LogInformation("Dackup start notify task ");

            var notifyList       = ParseNotifyFromConfig(cfg);
            var notifyResultList = new List<Task<NotifyResult>>();

            notifyList.ForEach(notify =>notifyResultList.Add(notify.NotifyAsync(statistics)));

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
                                storageConfig.OptionList.NullSafeSetTo<string>(s => task.Path = s, "path");
                                if (storageConfig.OptionList.ToList().Find(c => c.Name.ToLower() == "remove_threshold") != null)
                                {
                                    task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(storageConfig.OptionList.ToList().Find(c => c.Name.ToLower() == "remove_threshold").Value);
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
        private AliyunOssStorage PopulateAliyunOssStorage(StorageConfig storageConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<AliyunOssStorage>();

            storageConfig.OptionList.NullSafeSetTo<string>(s => task.Endpoint = s, "endpoint");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.AccessKeyId = s, "access_key_id");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.AccessKeySecret = s, "access_key_secret", "secret_access_key");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.BucketName = s, "bucket");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.PathPrefix = s, "path");

            if (storageConfig.OptionList?.ToList().Find(c => c.Name.ToLower() == "remove_threshold") != null)
            {
                task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(storageConfig.OptionList.ToList().Find(c => c.Name.ToLower() == "remove_threshold").Value);
            }

            return task;
        }
        private S3Storage PopulateS3Storage(StorageConfig storageConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<S3Storage>();

            storageConfig.OptionList.NullSafeSetTo<string>(s => task.PathPrefix = s, "path");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.Region = s, "region");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.AccessKeyId = s, "access_key_id");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.AccessKeySecret = s, "secret_access_key");
            storageConfig.OptionList.NullSafeSetTo<string>(s => task.BucketName = s, "bucket");

            if (storageConfig.OptionList?.ToList().Find(c => c.Name.ToLower() == "remove_threshold") != null)
            {
                task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(storageConfig.OptionList.ToList().Find(c => c.Name.ToLower() == "remove_threshold").Value);
            }

            return task;
        }
        private ArchiveBackupTask PopulateArchiveBackupTask(ArchiveConfig cfg)
        {
            var task             = ServiceProviderFactory.ServiceProvider.GetService<ArchiveBackupTask>();

            task.Name            = cfg.Name;
            task.IncludePathList = cfg.Includes;
            task.ExcludePathList = cfg.Excludes;

            return task;
        }
        private PostgresBackupTask PopulatePostgresBackupTask(DatabaseConfig dbConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<PostgresBackupTask>();
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Host = s, "host");
            dbConfig.OptionList.NullSafeSetTo<int>(s => task.Port = s, "port");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Database = s, "database");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.UserName = s, "username");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Password = s, "password");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.PathToPgDump = s, "path_to_pg_dump");
            if (dbConfig.AdditionalOptionList != null && dbConfig.AdditionalOptionList.Count > 0)
            {
                dbConfig.AdditionalOptionList.ToList().ForEach(c =>
                {
                    task.AddCommandOptions(c.Name, c.Value);
                });
            }

            return task;
        }
        private MySqlBackupTask PopulateMysqlBuckupTask(DatabaseConfig dbConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<MySqlBackupTask>();
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Host = s, "host");
            dbConfig.OptionList.NullSafeSetTo<int>(s => task.Port = s, "port");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Database = s, "database");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.UserName = s, "username");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Password = s, "password");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.PathToMysqlDump = s, "path_to_mysqldump");
            if (dbConfig.AdditionalOptionList != null && dbConfig.AdditionalOptionList.Count > 0)
            {
                dbConfig.AdditionalOptionList.ToList().ForEach(c =>
                {
                    task.AddCommandOptions(c.Name, c.Value);
                });
            }

            return task;
        }
        private MongoDBBackupTask PopulateMongoDBBackupTask(DatabaseConfig dbConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<MongoDBBackupTask>();
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Host = s, "host");
            dbConfig.OptionList.NullSafeSetTo<int>(s => task.Port = s, "port");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Database = s, "database");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.UserName = s, "username");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Password = s, "password");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.PathToMongoDump = s, "path_to_mongodump");
            if (dbConfig.AdditionalOptionList != null && dbConfig.AdditionalOptionList.Count > 0)
            {
                dbConfig.AdditionalOptionList.ToList().ForEach(c =>
                {
                    task.AddCommandOptions(c.Name, c.Value);
                });
            }

            return task;
        }
        private MsSqlBackupTask PopulateMsSqlBackupTask(DatabaseConfig dbConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<MsSqlBackupTask>();
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Host = s, "host");
            dbConfig.OptionList.NullSafeSetTo<int>(s => task.Port = s, "port");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Database = s, "database");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.UserName = s, "username");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Password = s, "password");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.PathToMssqlDump = s, "path_to_mssqldump");
            if (dbConfig.AdditionalOptionList != null && dbConfig.AdditionalOptionList.Count > 0)
            {
                dbConfig.AdditionalOptionList.ToList().ForEach(c =>
                {
                    task.AddCommandOptions(c.Name, c.Value);
                });
            }

            return task;
        }
        private SmtpEmailNotify PopulateEmailSmtpNotify(EmailNotifyConfig cfg)
        {
            var emailNotify       = ServiceProviderFactory.ServiceProvider.GetService<SmtpEmailNotify>();
            
            emailNotify.Enable    = cfg.Enable;
            emailNotify.OnFailure = cfg.OnFailure;
            emailNotify.OnSuccess = cfg.OnSuccess;
            emailNotify.OnWarning = cfg.OnWarning;

            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.From = s, "from");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.To = s, "to");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.Address = s, "address");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.Domain = s, "domain");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.UserName = s, "user_name");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.Password = s, "password");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.Authentication = s, "authentication");
            cfg.OptionList.NullSafeSetTo<bool>(s => emailNotify.EnableStarttls = s, "enable_starttls");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.CC = s, "cc");
            cfg.OptionList.NullSafeSetTo<string>(s => emailNotify.BCC = s, "bcc");
            cfg.OptionList.NullSafeSetTo<int>(port => emailNotify.Port = port, "port");

            return emailNotify;
        }
        private SlackNotify PopulateSlackNotify(SlackNotifyConfig cfg)
        {
            var slackNotify       = ServiceProviderFactory.ServiceProvider.GetService<SlackNotify>();
           
            slackNotify.Enable    = cfg.Enable;
            slackNotify.OnFailure = cfg.OnFailure;
            slackNotify.OnSuccess = cfg.OnSuccess;
            slackNotify.OnWarning = cfg.OnWarning;

            cfg.OptionList.NullSafeSetTo<string>(s => slackNotify.WebHookUrl = s, "webhook_url");
            cfg.OptionList.NullSafeSetTo<string>(s => slackNotify.Channel = s, "channel");
            cfg.OptionList.NullSafeSetTo<string>(s => slackNotify.Icon_emoji = s, "icon_emoji");
            cfg.OptionList.NullSafeSetTo<string>(s => slackNotify.UserName = s, "username");

            return slackNotify;
        }
        private DingtalkRobotNotify PopulateDingtalkRobotNotify(DingtalkRobotNotifyConfig cfg)
        {
            var dingtalkRobotNotify = ServiceProviderFactory.ServiceProvider.GetService<DingtalkRobotNotify>();
            cfg.OptionList.NullSafeSetTo<string>(s => dingtalkRobotNotify.WebHookUrl = s, "url");
            dingtalkRobotNotify.AtAll     = cfg.AtAll;
            dingtalkRobotNotify.Enable    = cfg.Enable;
            dingtalkRobotNotify.OnFailure = cfg.OnFailure;
            dingtalkRobotNotify.OnSuccess = cfg.OnSuccess;
            dingtalkRobotNotify.OnWarning = cfg.OnWarning;

            if (cfg.AtMobiles != null && cfg.AtMobiles.Count > 0)
            {
                dingtalkRobotNotify.AtMobiles = new List<string>();
                cfg.AtMobiles.ToList().ForEach(header =>
                {
                    dingtalkRobotNotify.AtMobiles.AddRange(header.Value.Split(';', StringSplitOptions.RemoveEmptyEntries));
                });
            }
            dingtalkRobotNotify.AtMobiles = dingtalkRobotNotify.AtMobiles.Distinct().ToList();

            return dingtalkRobotNotify;
        }
        private HttpPostNotify PopulateHttpPostNotify(HttpPostNotifyConfig cfg)
        {
            var httpPostNotify = ServiceProviderFactory.ServiceProvider.GetService<HttpPostNotify>();
            cfg.OptionList.NullSafeSetTo<string>(s => httpPostNotify.WebHookUrl = s, "url");
            
            httpPostNotify.Enable    = cfg.Enable;
            httpPostNotify.OnFailure = cfg.OnFailure;
            httpPostNotify.OnSuccess = cfg.OnSuccess;
            httpPostNotify.OnWarning = cfg.OnWarning;

            if (cfg.Headers != null)
            {
                httpPostNotify.Headers = new NameValueCollection();
                cfg.Headers.ToList().ForEach(header =>
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