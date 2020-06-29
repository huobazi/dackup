using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using Npgsql;

namespace dackup
{
    public class PostgresBackupTask : DatabaseBackupTask
    {
        private readonly ILogger logger;
        public PostgresBackupTask(ILogger<PostgresBackupTask> logger) : base("postgres")
        {
            this.logger = logger;
        }

        public string PathToPgDump { get; set; } = "pg_dump";
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5432;
        public string UserName { get; set; } = "postgres";
        public string Password { get; set; }
        public string Database { get; set; }
        protected override ILogger Logger
        {
            get { return this.logger; }
        }
        protected override bool CheckDbConnection()
        {
            logger.LogInformation($"Testing connection to '{UserName}@{Host}:{Port}/{Database}'...");

            var connectionString = $"Server={Host};Port={Port};User Id={UserName};Password={Password};Database={Database};";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch(Exception exception)
                {
                    logger.LogError(exception, "Can not connection !!!");
                    return false;
                }            
            }

            logger.LogInformation("Connection to DB established.");
            return true;
        }
        private (string resultFileName, string resultContent) GenerateOptionsToCommand()
        {
            this.RemoveCommandOptions("--host");
            this.RemoveCommandOptions("--h");
            this.RemoveCommandOptions("--port");
            this.RemoveCommandOptions("--p");
            this.RemoveCommandOptions("--username");
            this.RemoveCommandOptions("--U");

            var now = DateTime.Now;
            var defaultBackupFileName = $"databases_{Database}_{now:yyyy_MM_dd_HH_mm_ss}.backup";
            var dumpFile = Path.Join(DackupContext.Current.TmpPath, defaultBackupFileName);

            this.AddCommandOptions("--host", this.Host);
            this.AddCommandOptions("--port", this.Port.ToString());
            this.AddCommandOptions("--username", this.UserName);

            if (!CommandOptions.ContainsKey("--format") && !CommandOptions.ContainsKey("-F"))
            {
                this.AddCommandOptions("--format", "custom");
            }
            if (!CommandOptions.ContainsKey("--compress") && !CommandOptions.ContainsKey("-Z"))
            {
                this.AddCommandOptions("--compress", "6");
            }
            if (!CommandOptions.ContainsKey("--dbname") && !CommandOptions.ContainsKey("-d"))
            {
                this.AddCommandOptions("--dbname", this.Database);
            }
            if (!CommandOptions.ContainsKey("--file") && !CommandOptions.ContainsKey("-f"))
            {
                dumpFile = Path.Join(DackupContext.Current.TmpPath, defaultBackupFileName);
                this.AddCommandOptions("--file", dumpFile);
            }
            else
            {
                if (CommandOptions.ContainsKey("--file"))
                {
                    dumpFile = Path.Join(DackupContext.Current.TmpPath, $"{now:yyyy_MM_dd_HH_mm_ss}_{CommandOptions["--file"]}");
                    this.AddCommandOptions("--file", dumpFile);
                }
                else if (CommandOptions.ContainsKey("-f"))
                {
                    dumpFile = Path.Join(DackupContext.Current.TmpPath, $"{now:yyyy_MM_dd_HH_mm_ss}_{CommandOptions["-f"]}");
                    this.AddCommandOptions("-f", dumpFile);
                }
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
        protected override BackupTaskResult CreateNewBackup()
        {
            var (backupFile, cmdOptions) = GenerateOptionsToCommand();

            var processStartInfo = new ProcessStartInfo("bash", $"-c \"{PathToPgDump}  {cmdOptions} \"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            processStartInfo.Environment.Add("PGPASSWORD", Password);
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
        protected override bool CheckDbBackupCommand()
        {
            logger.LogInformation("Checking pg_dump existence...");

            var processStartInfo = new ProcessStartInfo("bash", $"-c \"{PathToPgDump} --help \"")
            {
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                logger.LogError($"pg_dump not found on path '{PathToPgDump}'.");
                return false;
            }

            logger.LogInformation("pg_dump found");

            return true;
        }
    }
}