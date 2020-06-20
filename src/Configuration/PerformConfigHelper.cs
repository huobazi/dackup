using System;
using System.IO;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

using Serilog;

using dackup.Configuration;

namespace dackup
{
    public static class PerformConfigHelper
    {
        public static PerformConfig LoadFrom(string fileName)
        {
            FileStream fs = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PerformConfig));
                              fs         = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                return (PerformConfig)serializer.Deserialize(fs);
            }
            catch (Exception exception)
            {
                Log.Error(Utils.FlattenException(exception));
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
        public static List<IBackupTask> ParseBackupTaskFromConfig(PerformConfig config)
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


        public static List<IStorage> ParseStorageFromConfig(PerformConfig config)
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
                                var task = new LocalStorage(storageConfig.OptionList?.Find(c => c.Name.ToLower() == "path")?.Value);
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

        public static List<INotify> ParseNotifyFromConfig(PerformConfig config)
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
                                EmailSmtpNotify email = PopulateEmailSmtpNotify(cfg);
                                tasks.Add(email);
                            }
                        }
                    });
                }
            }

            return tasks;
        }
        public static void GenerateMockupConfig(string fileName)
        {
            WriteResourceToFile("dackup.perform-config-mockup.config", fileName);
        }
        public static void WriteResourceToFile(string resourceName, string fileName)
        {
            Console.WriteLine($"====> Write mockup file to {fileName}");
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }

        private static AliyunOssStorage PopulateAliyunOssStorage(Storage storageConfig)
        {
            var endpoint        = storageConfig.OptionList?.Find(c => c.Name.ToLower() == "endpoint")?.Value;
            var accessKeyId     = storageConfig.OptionList?.Find(c => c.Name.ToLower() == "access_key_id")?.Value;
            var accessKeySecret = storageConfig.OptionList?.Find(c => c.Name.ToLower() == "secret_access_key")?.Value;
            var bucket          = storageConfig.OptionList?.Find(c => c.Name.ToLower() == "bucket")?.Value;
            var task            = new AliyunOssStorage(endpoint, accessKeyId, accessKeySecret, bucket);
            storageConfig.OptionList.NullSafeSetTo("path", path => task.PathPrefix = path);

            if (storageConfig.OptionList?.Find(c => c.Name.ToLower() == "remove_threshold") != null)
            {
                task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(storageConfig.OptionList.Find(c => c.Name.ToLower() == "remove_threshold").Value);
            }

            return task;
        }
        private static S3Storage PopulateS3Storage(Storage storageConfig)
        {
            var region          = storageConfig.OptionList?.Find(c => c.Name.ToLower() == "region")?.Value;
            var accessKeyId     = storageConfig.OptionList?.Find(c => c.Name.ToLower() == "access_key_id")?.Value;
            var accessKeySecret = storageConfig.OptionList?.Find(c => c.Name.ToLower() == "secret_access_key")?.Value;
            var bucket          = storageConfig.OptionList?.Find(c => c.Name.ToLower() == "bucket")?.Value;

            var task = new S3Storage(region, accessKeyId, accessKeySecret, bucket);
            storageConfig.OptionList.NullSafeSetTo("path", path => task.PathPrefix = path);

            if (storageConfig.OptionList?.Find(c => c.Name.ToLower() == "remove_threshold") != null)
            {
                task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(storageConfig.OptionList.Find(c => c.Name.ToLower() == "remove_threshold").Value);
            }

            return task;
        }

        private static ArchiveBackupTask PopulateArchiveBackupTask(Archive cfg)
        {
            var name         = cfg.Name;
            var includesList = cfg.Includes;
            var excludesList = cfg.Excludes;
            var task         = new ArchiveBackupTask(name, includesList, excludesList);

            return task;
        }
        private static PostgresBackupTask PopulatePostgresBackupTask(Database dbConfig)
        {
            var task = new PostgresBackupTask();
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
        private static MySqlBackupTask PopulateMysqlBuckupTask(Database dbConfig)
        {
            var task = new MySqlBackupTask();
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
        private static MongoDBBackupTask PopulateMongoDBBackupTask(Database dbConfig)
        {
            var task = new MongoDBBackupTask();
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
        private static MsSqlBackupTask PopulateMsSqlBackupTask(Database dbConfig)
        {
            var task = new MsSqlBackupTask();
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
        private static EmailSmtpNotify PopulateEmailSmtpNotify(Email cfg)
        {
            var from           = cfg.OptionList?.Find(c => c.Name == "from")?.Value;
            var to             = cfg.OptionList?.Find(c => c.Name == "to")?.Value;
            var address        = cfg.OptionList?.Find(c => c.Name == "address")?.Value;
            var domain         = cfg.OptionList?.Find(c => c.Name == "domain")?.Value;
            var userName       = cfg.OptionList?.Find(c => c.Name == "user_name")?.Value;
            var password       = cfg.OptionList?.Find(c => c.Name == "password")?.Value;
            var authentication = cfg.OptionList?.Find(c => c.Name == "authentication")?.Value;
            var enableStarttls = cfg.OptionList?.Find(c => c.Name == "enable_starttls")?.Value;
            var cc             = cfg.OptionList?.Find(c => c.Name == "cc")?.Value;
            var bcc            = cfg.OptionList?.Find(c => c.Name == "bcc")?.Value;

            var email = new EmailSmtpNotify(from, to, address, domain, userName, password,
                                            authentication, bool.Parse(enableStarttls), cc, bcc);

            email.Enable    = cfg.Enable;
            email.OnFailure = cfg.OnFailure;
            email.OnSuccess = cfg.OnSuccess;
            email.OnWarning = cfg.OnWarning;
            cfg.OptionList.NullSafeSetTo("port", port => email.Port = port);

            return email;
        }
        private static SlackNotify PopulateSlackNotify(Slack cfg)
        {
            var webhook_url     = cfg.OptionList?.Find(c => c.Name == "webhook_url")?.Value;
            var slack           = new SlackNotify(webhook_url);
                slack.Enable    = cfg.Enable;
                slack.OnFailure = cfg.OnFailure;
                slack.OnSuccess = cfg.OnSuccess;
                slack.OnWarning = cfg.OnWarning;
            cfg.OptionList.NullSafeSetTo("channel", channel => slack.Channel = channel);
            cfg.OptionList.NullSafeSetTo("icon_emoji", icon_emoji => slack.Icon_emoji = icon_emoji);
            cfg.OptionList.NullSafeSetTo("username", username => slack.UserName = username);

            return slack;
        }
        private static DingtalkRobotNotify PopulateDingtalkRobotNotify(DingtalkRobot cfg)
        {
            var webhook_url             = cfg.OptionList?.Find(c => c.Name == "url")?.Value;
            var dingtalkRobot           = new DingtalkRobotNotify(webhook_url);
                dingtalkRobot.AtAll     = cfg.AtAll;
                dingtalkRobot.Enable    = cfg.Enable;
                dingtalkRobot.OnFailure = cfg.OnFailure;
                dingtalkRobot.OnSuccess = cfg.OnSuccess;
                dingtalkRobot.OnWarning = cfg.OnWarning;
            if (cfg.AtMobiles != null && cfg.AtMobiles.Count > 0)
            {
                dingtalkRobot.AtMobiles = new List<string>();
                cfg.AtMobiles.ForEach(header =>
                {
                    dingtalkRobot.AtMobiles.AddRange(header.Value.Split(';', StringSplitOptions.RemoveEmptyEntries));
                });
            }
            dingtalkRobot.AtMobiles = dingtalkRobot.AtMobiles.Distinct().ToList();

            return dingtalkRobot;
        }
        private static HttpPostNotify PopulateHttpPostNotify(HttpPost cfg)
        {
            var webhook_url        = cfg.OptionList?.Find(c => c.Name == "url")?.Value;
            var httpPost           = new HttpPostNotify(webhook_url);
                httpPost.Enable    = cfg.Enable;
                httpPost.OnFailure = cfg.OnFailure;
                httpPost.OnSuccess = cfg.OnSuccess;
                httpPost.OnWarning = cfg.OnWarning;
            if (cfg.Headers != null)
            {
                httpPost.Headers = new NameValueCollection();
                cfg.Headers.ForEach(header =>
                {
                    httpPost.Headers[header.Name] = header.Value;
                });
            }
            var paramsList = cfg.OptionList?.Where(c => c.Name.ToLower() != "url")?.ToList();
            if (paramsList != null)
            {
                httpPost.Params = new NameValueCollection();
                paramsList.ForEach(param =>
                {
                    httpPost.Params[param.Name] = param.Value;
                });
            }

            return httpPost;
        }
        private static void NullSafeSetTo(this List<NameValueElement> list, string name, Action<string> setter)
        {
            var value = list?.Find(c => c.Name.ToLower() == name.ToLower())?.Value;
            if (value != null)
            {
                setter(value);
            }
        }
        private static void NullSafeSetTo(this List<NameValueElement> list, string name, Action<int> setter)
        {
            var value = list?.Find(c => c.Name.ToLower() == name.ToLower())?.Value;
            if (value != null)
            {
                setter(Convert.ToInt32(value));
            }
        }
    }
}
