using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using Dackup.Configuration;
using Dackup.Extensions;

namespace Dackup.Backup
{
    public static class BackupTaskFactory
    {
        public static ArchiveBackupTask CreateArchiveBackupTask(ArchiveConfig cfg)
        {
            var task             = ServiceProviderFactory.ServiceProvider.GetService<ArchiveBackupTask>();

            task.Name            = cfg.Name;
            task.IncludePathList = cfg.Includes;
            task.ExcludePathList = cfg.Excludes;

            return task;
        }
        public static PostgresBackupTask CreatePostgresBackupTask(DatabaseConfig dbConfig)
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
                dbConfig.AdditionalOptionList.ForEach(c =>
                {
                    task.AddCommandOptions(c.Name, c.Value);
                });
            }

            return task;
        }
        public static MySqlBackupTask CreateMysqlBuckupTask(DatabaseConfig dbConfig)
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
                dbConfig.AdditionalOptionList.ForEach(c =>
                {
                    task.AddCommandOptions(c.Name, c.Value);
                });
            }

            return task;
        }
        public static MongoDBBackupTask CreateMongoDBBackupTask(DatabaseConfig dbConfig)
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
                dbConfig.AdditionalOptionList.ForEach(c =>
                {
                    task.AddCommandOptions(c.Name, c.Value);
                });
            }

            return task;
        }
        public static MsSqlBackupTask CreateMsSqlBackupTask(DatabaseConfig dbConfig)
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
                dbConfig.AdditionalOptionList.ForEach(c =>
                {
                    task.AddCommandOptions(c.Name, c.Value);
                });
            }

            return task;
        }
        public static RedisBackupTask CreateRedisBackupTask(DatabaseConfig dbConfig)
        {
            var task = ServiceProviderFactory.ServiceProvider.GetService<RedisBackupTask>();

            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Host = s, "host");
            dbConfig.OptionList.NullSafeSetTo<int>(s => task.Port = s, "port");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.Password = s, "password");
            dbConfig.OptionList.NullSafeSetTo<string>(s => task.PathToRedisCLI = s, "path_to_redis_cli");

            return task;
        }
    }
}