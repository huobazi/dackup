using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text;
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
        private (string resultFileName, string resultContent) GenerateOptionsToCommand()
        {
            var now = DateTime.Now;
            var defaultBackupSQLName = $"databases_{Database}_{now:yyyy_MM_dd_HH_mm_ss}.sql";
            var dumpFile = Path.Join(DackupContext.Current.TmpPath, defaultBackupSQLName);

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
            var dumpTGZFileName = dumpfile + ".tar.gz";
            var processStartInfo = new ProcessStartInfo("bash", $"-c \"{PathToMysqlDump} {cmdOptions} \"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();
            process.WaitForExit();
            var code = process.ExitCode;

            Utils.CreateTarGZ(new List<string> { dumpfile, }, dumpTGZFileName);

            Log.Information($"{Database} backup completed. dump files : {dumpTGZFileName}");

            var result = new BackupTaskResult
            {
                Result = true,
                FilesList = new List<string> { dumpTGZFileName },
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