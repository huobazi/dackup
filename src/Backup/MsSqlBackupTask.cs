using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Dackup.Backup
{
    public class MsSqlBackupTask : DatabaseBackupTask
    {
        private readonly ILogger logger;
        public MsSqlBackupTask(ILogger<MsSqlBackupTask> logger) : base("mssql") => this.logger = logger;
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
        protected override bool CheckDbConnection()
        {
            logger.LogInformation($"Testing connection to SQL Server '{UserName}@{Host}:{Port}/{Database}'...");

            using (var connection = new SqlConnection($"Data Source={Host},{Port};Initial Catalog={Database};User Id={UserName};Password={Password};"))
            {
                try
                {
                    connection.Open();
                }
                catch(Exception exception)
                {
                    logger.LogError(exception, $"Can not connection to SQL Server '{UserName}@{Host}:{Port}/{Database}'!!!");
                    return false;
                }
            }
            logger.LogInformation($"Connection to SQL Server '{UserName}@{Host}:{Port}/{Database}' established.");
            
            return true;
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
        protected override BackupTaskResult CreateNewBackup()
        {
            var (dumpfile, cmdOptions) = GenerateOptionsToCommand();
            var dumpTgzFileName        = dumpfile + ".tar.gz";
            var processStartInfo = new ProcessStartInfo("bash", $"-c \"{PathToMssqlDump} {cmdOptions} \"")
            {
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();
            process.WaitForExit();
            var code = process.ExitCode;

            Utils.CreateTarGZ(new List<string> { dumpfile, }, dumpTgzFileName);

            logger.LogInformation($"SQL Server{Database} backup completed. dump files : {dumpTgzFileName}");

            var result = new BackupTaskResult
            {
                Result = true,
                FilesList = new List<string> { dumpTgzFileName },
            };

            return result;
        }
        protected override bool CheckDbBackupCommand()
        {
            logger.LogInformation("Checking sqlcmd existence...");

            var processStartInfo = new ProcessStartInfo("bash", $"-c \"{PathToMssqlDump} -? \"")
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
                logger.LogError($"sqlcmd not found on path '{PathToMssqlDump}'.");
                return false;
            }

            logger.LogInformation("sqlcmd found");

            return true;        
        }
    }
}