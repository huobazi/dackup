using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;
using MongoDB.Bson;

namespace dackup
{
    public class MongoDBBackupTask : DatabaseBackupTask
    {
        private readonly ILogger logger;

        public MongoDBBackupTask(ILogger<MongoDBBackupTask> logger) : base("mongodb")
        {
            this.logger = logger;
        }
        public string PathToMongoDump { get; set; } = "mongodump";
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 27017;
        public string UserName { get; set; } = "root";
        public string Password { get; set; }
        public string Database { get; set; }
        protected override ILogger Logger
        {
            get { return this.logger; }
        }
        public override void CheckDbConnection()
        {
            logger.LogInformation($"Testing connection to 'mongodb://{UserName}@{Host}:{Port}/{Database}'...");

            var client = new MongoClient($"mongodb://{UserName}:{Password}@{Host}:{Port}");
            var database = client.GetDatabase(Database);

            logger.LogInformation("Connection to DB established.");
        }
        private (string resultFileName, string resultContent) GenerateOptionsToCommand()
        {
            this.RemoveCommandOptions("--out"); // only support --archive option
            this.RemoveCommandOptions("--host");
            this.RemoveCommandOptions("--port");
            this.RemoveCommandOptions("--username");
            this.RemoveCommandOptions("--password");

            var now = DateTime.Now;
            var defaultBackupFileName = $"databases_{Database}_{now:yyyy_MM_dd_HH_mm_ss}.gz";
            var dumpFile = Path.Join(DackupContext.Current.TmpPath, defaultBackupFileName);

            this.AddCommandOptions("--host", this.Host);
            this.AddCommandOptions("--port", this.Port.ToString());
            this.AddCommandOptions("--username", this.UserName);
            this.AddCommandOptions("--password", this.Password);

            if (!CommandOptions.ContainsKey("--db"))
            {
                this.AddCommandOptions("--db", this.Database);
            }
            if (!CommandOptions.ContainsKey("--gzip"))
            {
                this.AddCommandOptions("--gzip", "");
            }
            if (!CommandOptions.ContainsKey("--archive"))
            {
                dumpFile = Path.Join(DackupContext.Current.TmpPath, defaultBackupFileName);
                this.AddCommandOptions("--archive", dumpFile);
            }
            else
            {
                dumpFile = Path.Join(DackupContext.Current.TmpPath, $"{now:yyyy_MM_dd_HH_mm_ss}_{CommandOptions["--archive"]}");
                this.AddCommandOptions("--archive", dumpFile);
            }
            var sb = new StringBuilder();
            foreach (var key in CommandOptions.Keys)
            {
                var value = CommandOptions[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    sb.Append($" {key} ");
                }
                else
                {
                    if (key.StartsWith("--"))
                    {
                        sb.Append($" {key}={value} ");
                    }
                    else
                    {
                        sb.Append($" {key} {value} ");
                    }
                }
            }
            return (dumpFile, sb.ToString());
        }
        public override BackupTaskResult CreateNewBackup()
        {
            var (backupFile, cmdOptions) = GenerateOptionsToCommand();

            var processStartInfo = new ProcessStartInfo("bash", $"-c \"{PathToMongoDump}  {cmdOptions} \"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();

            process.WaitForExit();
            var code = process.ExitCode;

            logger.LogInformation($"{Database} backup completed. dump files : {backupFile}");

            var result = new BackupTaskResult
            {
                Result = true,
                FilesList = new List<string> { backupFile },
            };

            return result;
        }
        private bool CheckPgDump()
        {
            logger.LogInformation("Checking mongodump existence...");

            var process = Process.Start(PathToMongoDump, "--help");
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                logger.LogInformation($"mongodump not found on path '{PathToMongoDump}'.");
                return false;
            }

            logger.LogInformation("mongodump found");

            return true;
        }
    }
}