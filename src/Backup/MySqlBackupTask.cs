using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using MySql.Data.MySqlClient;

namespace dackup
{
    public class MySqlBackupTask: DatabaseBackupTask
    {
        private readonly ILogger logger;

        public MySqlBackupTask(ILogger<MySqlBackupTask> logger): base("mysql") {
            this.logger = logger;
         }
        public string PathToMysqlDump { get; set; } = "mysqldump";
        public string Host { get; set; }            = "localhost";
        public int Port { get; set; }               = 3306;
        public string UserName { get; set; }        = "root";
        public string Password { get; set; }
        public string Database { get; set; }
        protected override ILogger Logger
        {
            get { return this.logger; }
        }
        public override void CheckDbConnection()
        {
            logger.LogInformation($"Testing connection to '{UserName}@{Host}:{Port}/{Database}'...");

            using (var connection = new MySqlConnection($"Server={Host};Port={Port};Database={Database};User Id={UserName};Password={Password};"))
            {
                connection.Open();
            }
            logger.LogInformation("Connection to DB established.");
        }
        private (string resultFileName, string resultContent) GenerateOptionsToCommand()
        {
            this.RemoveCommandOptions("--host");
            this.RemoveCommandOptions("--h");
            this.RemoveCommandOptions("--port");
            this.RemoveCommandOptions("--P");
            this.RemoveCommandOptions("--user");
            this.RemoveCommandOptions("--u");
            this.RemoveCommandOptions("--password");
            this.RemoveCommandOptions("--P");

            var now                  = DateTime.Now;
            var defaultBackupSQLName = $"databases_{Database}_{now:yyyy_MM_dd_HH_mm_ss}.sql";
            var dumpFile             = Path.Join(DackupContext.Current.TmpPath, defaultBackupSQLName);
            this.AddCommandOptions("--host", this.Host);
            this.AddCommandOptions("--port", this.Port.ToString());
            this.AddCommandOptions("--user", this.UserName);
            this.AddCommandOptions("--password", this.Password);

            if (!CommandOptions.ContainsKey("--databases") && !CommandOptions.ContainsKey("-B"))
            {
                this.AddCommandOptions("--databases", this.Database);
            }
            if (!CommandOptions.ContainsKey("--result-file") && !CommandOptions.ContainsKey("-r"))
            {
                dumpFile = Path.Join(DackupContext.Current.TmpPath, defaultBackupSQLName);
                this.AddCommandOptions("--result-file", dumpFile);
            }
            else
            {
                if (CommandOptions.ContainsKey("--result-file"))
                {
                    dumpFile = Path.Join(DackupContext.Current.TmpPath, $"{now:yyyy_MM_dd_HH_mm_ss}_{CommandOptions["--result-file"]}");
                    this.AddCommandOptions("--result-file", dumpFile);
                }
                else if (CommandOptions.ContainsKey("-r"))
                {
                    dumpFile = Path.Join(DackupContext.Current.TmpPath, $"{now:yyyy_MM_dd_HH_mm_ss}_{CommandOptions["-r"]}");
                    this.AddCommandOptions("-r", dumpFile);
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
                    if (key == "--databases" || key == "-B")
                    {
                        sb.Append($" {key} {value} ");
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
            }
            return (dumpFile, sb.ToString());
        }
        public override BackupTaskResult CreateNewBackup()
        {
            var (dumpfile, cmdOptions) = GenerateOptionsToCommand();
            var dumpTGZFileName        = dumpfile + ".tar.gz";
            var processStartInfo       = new ProcessStartInfo("bash", $"-c \"{PathToMysqlDump} {cmdOptions} \"")
            {
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();
            process.WaitForExit();
            var code = process.ExitCode;

            Utils.CreateTarGZ(new List<string> { dumpfile, }, dumpTGZFileName);

            logger.LogInformation($"{Database} backup completed. dump files : {dumpTGZFileName}");

            var result = new BackupTaskResult
            {
                Result    = true,
                FilesList = new List<string> { dumpTGZFileName },
            };

            return result;
        }
        private bool CheckPgDump()
        {
            logger.LogInformation("Checking mysqldump existence...");

            var process = Process.Start(PathToMysqlDump, "--help");
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                logger.LogInformation($"mysqldump not found on path '{PathToMysqlDump}'.");
                return false;
            }

            logger.LogInformation("mysqldump found");

            return true;
        }
    }
}