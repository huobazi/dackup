using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Serilog;
using Serilog.Configuration;

using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;

namespace dackup
{
    class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .CreateLogger();

            var app = new CommandLineApplication
            {
                Name = "dackup",
                Description = "A backup app for your server or database or desktop",
            };

            app.HelpOption(inherited: true);

            app.Command("gen", genCmd =>
            {
                genCmd.Description = "Generate a config file";

                var modelName = genCmd.Argument("model", "Name of the model").IsRequired();

                genCmd.OnExecute(() =>
                {
                    Console.WriteLine("modleName = " + modelName.Value);
                    return 1;
                });
            });

            app.Command("perform", performCmd =>
            {
                performCmd.Description = "Performing your backup by config";

                var configFile = performCmd.Option("--config-file  <FILE>", "Required. The File name of the config.", CommandOptionType.SingleValue)
                                            .IsRequired()
                                            .Accepts(v => v.ExistingFile());
                var logPath = performCmd.Option("--log-path  <PATH>", "op. The File path of the log.", CommandOptionType.SingleValue);
                var tmpPath = performCmd.Option("--tmp-path  <PATH>", "op. The tmp path.", CommandOptionType.SingleValue);

                BackupContext.Create(Path.Join(logPath.Value(), "dackup.log"), tmpPath.Value());

                performCmd.OnExecute(() =>
                {
                    var config = configFile.Value();

                    Log.Information("======== Dackup start ========");

                    // run backup
                    var taskList = ParseBackupTaskFromConfig(config);
                    taskList.ForEach(task=>{
                        var result = task.Backup();
                        if(result.Result)
                        {
                            BackupContext.Current.AddToGenerateFilesList(result.FilesList);
                        }
                    });

                    Log.Information("======== Dackup start storage task ========");

                    // run store
                    var storageList = ParseStorageFromConfig(config);
                    storageList.ForEach(storage=>{
                        BackupContext.Current.GenerateFilesList.ForEach(file=>{
                            storage.Upload(file);
                        });
                        storage.Purge();
                    });

                    Log.Information("======== Dackup start notify task ========");

                    // run notify
                    var notifyList = ParseNotifyFromConfig(config);
                    notifyList.ForEach(notify=>{
                        notify.Notify();
                    });

                    Log.CloseAndFlush();
                    
                    Log.Information("======== Dackup done ========");

                    return 1;
                });

            });

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 1;
            });

            return app.Execute(args);
        }

        private static List<IBackupTask> ParseBackupTaskFromConfig(string configFile)
        {
            return null;
        }
        private static List<IStorage> ParseStorageFromConfig(string configFile)
        {
            return null;
        }
        private static List<INotify> ParseNotifyFromConfig(string configFile)
        {
            return null;
        }
        
    }
}