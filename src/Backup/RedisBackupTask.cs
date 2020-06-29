using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace dackup
{
    public class RedisBackupTask: DatabaseBackupTask
    {
        private readonly ILogger logger;
        public RedisBackupTask(ILogger<MySqlBackupTask> logger): base("redis") 
        {
            this.logger = logger;
        }
        public string PathToRedisCLI { get; set; } = "redis-cli";
        public string Host { get; set; }            = "127.0.0.1";
        public int Port { get; set; }               = 6379;
        public string Password { get; set; }
        protected override ILogger Logger
        {
            get { return this.logger; }
        }
        protected override bool CheckDbConnection()
        {
            logger.LogInformation($"Testing redis connection to '{Host}:{Port}'...");
            try
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{Host}:{Port}");
                IDatabase db = redis.GetDatabase();
                db.StringSet("test_connection_key", "abcdefg");
                db.StringGet("test_connection_key");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Can not connection to redis '{Host}:{Port}' !!!");
                return false;
            }
            logger.LogInformation($"Connection to redis '{Host}:{Port}' established.");

            return true;
        }
        private (string resultFileName, string resultContent) GenerateOptionsToCommand()
        {
            this.RemoveCommandOptions("-h");
            this.RemoveCommandOptions("-p");
            this.RemoveCommandOptions("-a");

            var now                  = DateTime.Now;
            var defaultBackupRdbFileName = $"redis_{Host}_{Port}_{now:yyyy_MM_dd_HH_mm_ss}.rdb";
            var dumpFile             = Path.Join(DackupContext.Current.TmpPath, defaultBackupRdbFileName);
            
            this.AddCommandOptions("-h", this.Host);
            this.AddCommandOptions("-p", this.Port.ToString());
            this.AddCommandOptions("-a", this.Password);
    
            
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
                    sb.Append($" {key} {value} ");                           
                }
            }

            return (dumpFile, sb.ToString());
        }
        protected override BackupTaskResult CreateNewBackup()
        {
            var (dumpfile, cmdOptions) = GenerateOptionsToCommand();
            var dumpTGZFileName        = dumpfile + ".tar.gz";
            var processSaveStartInfo       = new ProcessStartInfo("bash", $"-c \"{PathToRedisCLI} {cmdOptions} save \"")
            {
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            var process = new Process { StartInfo = processSaveStartInfo };
            process.Start();
            process.WaitForExit();
            var code = process.ExitCode;

            var processRdbStartInfo       = new ProcessStartInfo("bash", $"-c \"{PathToRedisCLI} {cmdOptions} --rdb {dumpfile} \"")
            {
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            var processRdb = new Process { StartInfo = processRdbStartInfo };
            processRdb.Start();
            processRdb.WaitForExit();
            var codeRdb = processRdb.ExitCode; 

            Utils.CreateTarGZ(new List<string> { dumpfile, }, dumpTGZFileName);

            logger.LogInformation($"Redis {Host}:{Port} backup completed. dump files : {dumpTGZFileName}");

            var result = new BackupTaskResult
            {
                Result    = true,
                FilesList = new List<string> { dumpTGZFileName },
            };

            return result;
        }
        protected override bool CheckDbBackupCommand()
        {
            logger.LogInformation("Checking redis-cli existence...");

            var processStartInfo = new ProcessStartInfo("bash", $"-c \"{PathToRedisCLI} --version \"")
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
                logger.LogError($"redis-cli not found on path '{PathToRedisCLI}'.");
                return false;
            }

            logger.LogInformation("redis-cli found");

            return true;
        }
    }
}