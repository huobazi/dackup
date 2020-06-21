using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace dackup
{
    public class MsSqlBackupTask : DatabaseBackupTask
    {
        private readonly ILogger logger;

        public MsSqlBackupTask(ILogger<MsSqlBackupTask> logger) : base("mssql")
        {
            this.logger = logger;
        }

        public string PathToMssqlDump { get; set; } = "sqlcmd";
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1433;
        public string UserName { get; set; } = "sa";
        public string Password { get; set; }
        public string Database { get; set; }
        protected override ILogger Logger
        {
            get { return this.logger; }
        }

        public override void CheckDbConnection()
        {
            logger.LogInformation($"Testing connection to '{UserName}@{Host}:{Port}/{Database}'...");

            using (var connection = new SqlConnection($"Data Source={Host},{Port};Initial Catalog={Database};User Id={UserName};Password={Password};"))
            {
                connection.Open();
            }
            logger.LogInformation("Connection to DB established.");
        }

        private (string resultFileName, string resultContent) GenerateOptionsToCommand()
        {
            RemoveCommandOptions("--host");
            RemoveCommandOptions("--h");
            RemoveCommandOptions("--port");
            RemoveCommandOptions("--P");
            RemoveCommandOptions("--user");
            RemoveCommandOptions("--u");
            RemoveCommandOptions("--password");
            RemoveCommandOptions("--P");
            RemoveCommandOptions("--database");
            RemoveCommandOptions("--D");

            var now = DateTime.Now;
            var defaultBackupSqlName = $"databases_{Database}_{now:yyyy_MM_dd_HH_mm_ss}.bak";
            var dumpFile = Path.Join(DackupContext.Current.TmpPath, defaultBackupSqlName);
            AddCommandOptions("-S", Host + "," + Port);
            if (!CommandOptions.ContainsKey("-E"))
            {
                AddCommandOptions("-U", UserName);
                AddCommandOptions("-P", Password);
            }
            AddCommandOptions("-Q", $"BACKUP DATABASE [{Database}] TO DISK = N'{dumpFile}'");

            var sb = new StringBuilder();
            foreach (var key in CommandOptions.Keys)
            {
                var value = CommandOptions[key];
                sb.Append(string.IsNullOrWhiteSpace(value) ? $" {key} " : $" {key} {value} ");
            }
            return (dumpFile, sb.ToString());
        }
        public override BackupTaskResult CreateNewBackup()
        {
            var (dumpfile, cmdOptions) = GenerateOptionsToCommand();
            var dumpTgzFileName = dumpfile + ".tar.gz";
            var processStartInfo = new ProcessStartInfo("bash", $"-c \"{PathToMssqlDump} {cmdOptions} \"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();
            process.WaitForExit();
            var code = process.ExitCode;

            Utils.CreateTarGZ(new List<string> { dumpfile, }, dumpTgzFileName);

            logger.LogInformation($"{Database} backup completed. dump files : {dumpTgzFileName}");

            var result = new BackupTaskResult
            {
                Result = true,
                FilesList = new List<string> { dumpTgzFileName },
            };

            return result;
        }
    }
}