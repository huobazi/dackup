using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

using Serilog;
using Npgsql;

namespace dackup
{
    public class PostgresBackupTask : DatabaseBackupTask
    {
        public PostgresBackupTask() : base("postgres") { }

        public string PathToPgDump { get; set; } = "pg_dump";
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5432;
        public string UserName { get; set; } = "postgres";
        public string Password { get; set; }
        public string Database { get; set; }

        public override void CheckDbConnection()
        {
            Log.Information($"Testing connection to '{UserName}@{Host}:{Port}/{Database}'...");

            using (var connection = new NpgsqlConnection($"Server={Host};Port={Port};Database={Database};User Id={UserName};Password={Password};"))
            {
                connection.Open();
            }
            Log.Information("Connection to DB established.");
        }
        public override BackupTaskResult CreateNewBackup()
        {
            var backupName = $"databases_{Database}_{DateTime.Now:s}.tar.gz";
            var backupFile = Path.Join(DackupContext.Current.TmpPath, backupName);
            var processStartInfo = new ProcessStartInfo("bash",
                                                        $"-c \"{PathToPgDump} -h {Host} -p {Port} -U {UserName} -d {Database} -F tar | gzip > {backupFile}\"")
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
            Log.Information("Checking pg_dump existence...");

            var process = Process.Start(PathToPgDump, "--help");
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                Log.Information($"pg_dump not found on path '{PathToPgDump}'.");
                return false;
            }

            Log.Information("pg_dump found");

            return true;
        }


    }
}