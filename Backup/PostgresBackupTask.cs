using System;
using System.IO;
using System.Text;
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
        private (string resultFileName, string resultContent) GenerateOptionsToCommand()
        {
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
        public override BackupTaskResult CreateNewBackup()
        {
            var (backupFile , cmdOptions) = GenerateOptionsToCommand();

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