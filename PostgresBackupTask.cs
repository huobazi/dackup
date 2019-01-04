using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

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
            Console.WriteLine($"Testing connection to '{UserName}@{Host}:{Port}/{Database}'...");

            using (var connection = new NpgsqlConnection($"Server={Host};Port={Port};Database={Database};User Id={UserName};Password={Password};"))
            {
                connection.Open();
            }
            Console.WriteLine("Connection to DB established.");
        }
        public override BackupTaskResult CreateNewBackup()
        {

            var backupName = $"{Database}_{DateTime.UtcNow:s}.tar.gz";
            var backupFile = Path.Join(BackupContext.Current.TmpPath, backupName);
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

            Console.WriteLine("Creating new backup completed.");

            var result = new BackupTaskResult
            {
                Result = true,
                FilesList = new List<string> { backupFile },
            };

            return result;
        }
        private bool CheckPgDump()
        {
            Console.WriteLine("Checking pg_dump existence...");

            var process = Process.Start(PathToPgDump, "--help");
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                Console.WriteLine($"pg_dump not found on path '{PathToPgDump}'.");
                return false;
            }

            Console.WriteLine("pg_dump found");

            return true;
        }


    }
}