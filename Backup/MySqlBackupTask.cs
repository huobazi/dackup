using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

using Serilog;

using MySql.Data.MySqlClient;

namespace dackup
{
    public class MySqlBackupTask : DatabaseBackupTask
    {
        public MySqlBackupTask() : base("mysql") { }
        public string PathToMysqlDump { get; set; } = "mysqldump";
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 3306;
        public string UserName { get; set; } = "root";
        public string Password { get; set; }
        public string Database { get; set; }

        public override void CheckDbConnection()
        {
            Log.Information($"Testing connection to '{UserName}@{Host}:{Port}/{Database}'...");

            using (var connection = new MySqlConnection($"Server={Host};Port={Port};Database={Database};User Id={UserName};Password={Password};"))
            {
                connection.Open();
            }
            Log.Information("Connection to DB established.");
        }
        public override BackupTaskResult CreateNewBackup()
        {
            var backupName = $"databases_{Database}_{DateTime.Now:s}.tar.gz";
            var gzipCmd = "| gzip -c";
            if(Host.ToLower() != "localhost" || Host.ToLower() != "127.0.0.1")
            {
                backupName = $"databases_{Database}_{DateTime.Now:s}.sql";
                gzipCmd = string.Empty;
            }
            var backupFile = Path.Join(DackupContext.Current.TmpPath, backupName);
            var processStartInfo = new ProcessStartInfo("bash",
                                                        $"-c \"{PathToMysqlDump} --host={Host} --port={Port} --user={UserName} --password{Password} {Database} {gzipCmd} > {backupFile}\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();

            process.WaitForExit();
            var code = process.ExitCode;

            Log.Information($"{Database} backup completed. dump files : {backupFile}");

            var result = new BackupTaskResult
            {
                Result = true,
                FilesList = new List<string> { backupFile },
            };

            return result;
        }
        private bool CheckPgDump()
        {
            Log.Information("Checking mysqldump existence...");

            var process = Process.Start(PathToMysqlDump, "--help");
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                Log.Information($"mysqldump not found on path '{PathToMysqlDump}'.");
                return false;
            }

            Log.Information("mysqldump found");

            return true;
        }
    }
}