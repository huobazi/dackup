using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using Dackup.Configuration;
using Dackup.Extensions;
using Dackup.Backup;
using Dackup.Storage;
using Dackup.Notify;

namespace Dackup
{
    public class DackupApplication
    {
        private readonly ILogger logger;
        public DackupApplication(ILogger<DackupApplication> logger) => this.logger = logger;
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
        private List<IBackupTask> ParseBackupTaskFromConfig(PerformConfig config)
        {
            var tasks = new List<IBackupTask>();
            if (config != null)
            {
                if (config.Archives != null && config.Archives.Count > 0)
                {
                    config.Archives.ForEach(cfg =>
                    {
                        ArchiveBackupTask task = BackupTaskFactory.CreateArchiveBackupTask(cfg);
                        tasks.Add(task);
                    });
                }
                if (config.Databases != null && config.Databases.Count > 0)
                {
                    config.Databases.ForEach(dbConfig =>
                    {
                        if (dbConfig.Enable)
                        {
                            if (dbConfig.Type.ToLower().Trim().In("postgres", "postgresql"))
                            {
                                PostgresBackupTask task = BackupTaskFactory.CreatePostgresBackupTask(dbConfig);
                                tasks.Add(task);
                            }
                            else if (dbConfig.Type.ToLower().Trim() == "mysql")
                            {
                                MySqlBackupTask task = BackupTaskFactory.CreateMysqlBuckupTask(dbConfig);
                                tasks.Add(task);
                            }
                            else if (dbConfig.Type.ToLower().Trim().In("mongo", "mongodb"))
                            {
                                MongoDBBackupTask task = BackupTaskFactory.CreateMongoDBBackupTask(dbConfig);
                                tasks.Add(task);
                            }
                            else if (dbConfig.Type.ToLower().Trim().In("sqlserver", "mssql"))
                            {
                                MsSqlBackupTask task = BackupTaskFactory.CreateMsSqlBackupTask(dbConfig);
                                tasks.Add(task);
                            }
                            else if (dbConfig.Type.ToLower().Trim() == "redis")
                            {
                                RedisBackupTask task = BackupTaskFactory.CreateRedisBackupTask(dbConfig);
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
                                LocalStorage task = StorageFactory.CreateLocalStorage(storageConfig);
                                tasks.Add(task);
                            }
                            else if (storageConfig.Type.ToLower().Trim() == "ftp")
                            {
                                FTPStorage task = StorageFactory.CreateFTPStorage(storageConfig);
                                tasks.Add(task);
                            }
                            else if (storageConfig.Type.ToLower().Trim() == "scp")
                            {
                                SCPStorage task = StorageFactory.CreateSCPStorage(storageConfig);
                                tasks.Add(task);
                            }
                            else if (storageConfig.Type.ToLower().Trim().In("s3", "aws_s3", "aws-s3"))
                            {
                                S3Storage task = StorageFactory.CreateS3Storage(storageConfig);
                                tasks.Add(task);
                            }
                            else if (storageConfig.Type.ToLower().Trim().In("aliyun_oss", "aliyun-oss"))
                            {
                                AliyunOssStorage task = StorageFactory.CreateAliyunOssStorage(storageConfig);
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
                            HttpPostNotify httpPost = NotifyFactory.CreateHttpPostNotify(cfg);
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
                            DingtalkRobotNotify dingtalkRobot = NotifyFactory.CreateDingtalkRobotNotify(cfg);
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
                            SlackNotify slack = NotifyFactory.CreateSlackNotify(cfg);
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
                                SmtpEmailNotify email = NotifyFactory.CreateEmailSmtpNotify(cfg);
                                tasks.Add(email);
                            }
                        }
                    });
                }
            }

            return tasks;
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
    }
}