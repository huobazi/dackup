using System;
using System.IO;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;


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
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                return (PerformConfig)serializer.Deserialize(fs);
            }
            catch (Exception)
            {
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
                if (config.Archives != null)
                {
                    var includesList = config.Archives.Includes;
                    var excludesList = config.Archives.Excludes;
                    var task = new ArchiveBackupTask(includesList, excludesList);
                    tasks.Add(task);
                }
                if (config.Databases != null && config.Databases != null)
                {
                    config.Databases.ForEach(dbConfig =>
                    {
                        if (dbConfig.Type.ToLower().Trim() == "postgres")
                        {
                            var task = new PostgresBackupTask();
                            task.Host = dbConfig.OptionList.Find(c => c.Name.ToLower() == "host").Value;
                            task.Database = dbConfig.OptionList.Find(c => c.Name.ToLower() == "database").Value;
                            task.UserName = dbConfig.OptionList.Find(c => c.Name.ToLower() == "username").Value;
                            task.Password = dbConfig.OptionList.Find(c => c.Name.ToLower() == "password").Value;
                            task.Port = int.Parse(dbConfig.OptionList.Find(c => c.Name.ToLower() == "port").Value);
                            task.PathToPgDump = dbConfig.OptionList.Find(c => c.Name.ToLower() == "path_to_pg_dump".ToLower()).Value;
                            tasks.Add(task);
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
                        if (storageConfig.Type.ToLower().Trim() == "local")
                        {
                            var task = new LocalStorage(storageConfig.OptionList.Find(c => c.Name.ToLower() == "path").Value);
                            if (storageConfig.OptionList.Find(c => c.Name.ToLower() == "remove_threshold") != null)
                            {
                                task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(storageConfig.OptionList.Find(c => c.Name.ToLower() == "remove_threshold").Value);
                            }
                            tasks.Add(task);
                        }
                        if (storageConfig.Type.ToLower().Trim() == "s3")
                        {
                            var region = storageConfig.OptionList.Find(c => c.Name.ToLower() == "region").Value;
                            var accessKeyId = storageConfig.OptionList.Find(c => c.Name.ToLower() == "access_key_id").Value;
                            var accessKeySecret = storageConfig.OptionList.Find(c => c.Name.ToLower() == "secret_access_key").Value;
                            var bucket = storageConfig.OptionList.Find(c => c.Name.ToLower() == "bucket").Value;

                            var task = new S3Storage(region, accessKeyId, accessKeySecret, bucket);
                            if (storageConfig.OptionList.Find(c => c.Name.ToLower() == "path") != null)
                            {
                                task.PathPrefix = storageConfig.OptionList.Find(c => c.Name.ToLower() == "path").Value.Trim('/').Trim('\\');
                            }
                            if (storageConfig.OptionList.Find(c => c.Name.ToLower() == "remove_threshold") != null)
                            {
                                task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(storageConfig.OptionList.Find(c => c.Name.ToLower() == "remove_threshold").Value);
                            }
                            tasks.Add(task);
                        }
                        if (storageConfig.Type.ToLower().Trim() == "aliyun_oss")
                        {
                            var endpoint = storageConfig.OptionList.Find(c => c.Name.ToLower() == "endpoint").Value;
                            var accessKeyId = storageConfig.OptionList.Find(c => c.Name.ToLower() == "access_key_id").Value;
                            var accessKeySecret = storageConfig.OptionList.Find(c => c.Name.ToLower() == "secret_access_key").Value;
                            var bucket = storageConfig.OptionList.Find(c => c.Name.ToLower() == "bucket").Value;

                            var task = new AliyunOssStorage(endpoint, accessKeyId, accessKeySecret, bucket);
                            if (storageConfig.OptionList.Find(c => c.Name.ToLower() == "path") != null)
                            {
                                task.PathPrefix = storageConfig.OptionList.Find(c => c.Name.ToLower() == "path").Value.Trim('/').Trim('\\');
                            }
                            if (storageConfig.OptionList.Find(c => c.Name.ToLower() == "remove_threshold") != null)
                            {
                                task.RemoveThreshold = Utils.ConvertRemoveThresholdToDateTime(storageConfig.OptionList.Find(c => c.Name.ToLower() == "remove_threshold").Value);
                            }
                            tasks.Add(task);
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
                if (config.Notifiers != null && config.Notifiers.HttpPost != null)
                {
                    var cfg = config.Notifiers.HttpPost;
                    if (cfg.Enable)
                    {
                        var webhook_url = cfg.OptionList.Find(c => c.Name == "url").Value;
                        var httpPost = new HttpPostNotify(webhook_url);
                        httpPost.Enable = cfg.Enable;
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
                        var paramsList = cfg.OptionList.Where(c => c.Name.ToLower() != "url").ToList();
                        if (paramsList != null)
                        {
                            httpPost.Params = new NameValueCollection();
                            paramsList.ForEach(param =>
                            {
                                httpPost.Params[param.Name] = param.Value;
                            });
                        }
                        tasks.Add(httpPost);
                    }
                }
                if (config.Notifiers != null && config.Notifiers.Slack != null)
                {
                    var cfg = config.Notifiers.Slack;
                    if (cfg.Enable)
                    {
                        var webhook_url = cfg.OptionList.Find(c => c.Name == "webhook_url").Value;
                        var slack = new SlackNotify(webhook_url);
                        slack.Enable = cfg.Enable;
                        slack.OnFailure = cfg.OnFailure;
                        slack.OnSuccess = cfg.OnSuccess;
                        slack.OnWarning = cfg.OnWarning;

                        if (cfg.OptionList.Find(c => c.Name.ToLower() == "channel") != null)
                        {
                            slack.Channel = cfg.OptionList.Find(c => c.Name.ToLower() == "channel").Value;
                        }
                        if (cfg.OptionList.Find(c => c.Name.ToLower() == "icon_emoji") != null)
                        {
                            slack.Icon_emoji = cfg.OptionList.Find(c => c.Name.ToLower() == "icon_emoji").Value;
                        }
                        if (cfg.OptionList.Find(c => c.Name.ToLower() == "username") != null)
                        {
                            slack.UserName = cfg.OptionList.Find(c => c.Name.ToLower() == "username").Value;
                        }
                        tasks.Add(slack);
                    }
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
    }
}