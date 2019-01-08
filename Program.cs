using System;
using System.IO;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Threading.Tasks;

using Serilog;

using McMaster.Extensions.CommandLineUtils;

namespace dackup
{
    class Program
    {
        public static int Main(string[] args)
        {

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
                    var fileName = Path.Combine(Environment.CurrentDirectory, modelName.Value + ".config");
                    PerformConfigHelper.GenerateMockupConfig(fileName);
                    return 1;
                });
            });

            app.Command("perform", performCmd =>
            {
                performCmd.Description = "Performing your backup by config";

                var configFile = performCmd.Option("--config-file  <FILE>", "Required. The File name of the config.", CommandOptionType.SingleValue)
                                            .IsRequired()
                                            .Accepts(v => v.ExistingFile());
                var logPathConfig = performCmd.Option("--log-path  <PATH>", "op. The File path of the log.", CommandOptionType.SingleValue);
                var tmpPathConfig = performCmd.Option("--tmp-path  <PATH>", "op. The tmp path.", CommandOptionType.SingleValue);


                performCmd.OnExecute(() =>
                {
                    var logPath = logPathConfig.Value();
                    var tmpPath = tmpPathConfig.Value();
                    if (string.IsNullOrEmpty(tmpPath))
                    {
                        tmpPath = "tmp";
                    }

                    var tmpWorkDirPath = Path.Combine(tmpPath, $"{DateTime.UtcNow:s}");
                    BackupContext.Create(Path.Join(logPath, "dackup.log"), tmpWorkDirPath);

                    Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File(BackupContext.Current.LogFile, rollingInterval: RollingInterval.Month, rollOnFileSizeLimit: true)
                    .CreateLogger();

                    Log.Information($" tmp: {BackupContext.Current.TmpPath}");
                    Log.Information($" log: {BackupContext.Current.LogFile}");
                    var configFilePath = configFile.Value();

                    var performConfig = PerformConfigHelper.LoadFrom(configFilePath);

                    Log.Information(" Step 1: Dackup start backup task ");

                    Directory.CreateDirectory(BackupContext.Current.TmpPath);

                    // run backup
                    var backupTaskList = PerformConfigHelper.ParseBackupTaskFromConfig(performConfig);
                    var backupTaskResult = new List<Task<BackupTaskResult>>();
                    backupTaskList.ForEach(task =>
                    {
                        var result = task.BackupAsync();
                        backupTaskResult.Add(result);
                    });
                    var backupTasks = Task.WhenAll(backupTaskResult.ToArray());
                    try
                    {
                        backupTasks.Wait();
                    }
                    catch (AggregateException)
                    {
                    }

                    Log.Information("Dackup start storage task ");
                    
                    // run store
                    var storageList = PerformConfigHelper.ParseStorageFromConfig(performConfig);
                    storageList.ForEach(storage =>
                    {
                        BackupContext.Current.GenerateFilesList.ForEach(file =>
                        {
                            storage.UploadAsync(file);
                        });
                        storage.PurgeAsync();
                    });

                    Log.Information("Dackup start notify task ");

                    // run notify
                    var notifyList = PerformConfigHelper.ParseNotifyFromConfig(performConfig);
                    string notifyMessage = $"Dackup {DateTime.Now} Sucess";

                    notifyList.ForEach(notify =>
                    {
                        notify.NotifyAsync(notifyMessage);
                    });
                                        
                    Log.Information("Dackup clean tmp folder ");

                    var di = new DirectoryInfo(BackupContext.Current.TmpPath);
                    foreach (var file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (var dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    Directory.Delete(BackupContext.Current.TmpPath);

                    Log.Information("Dackup done ");
                    Log.CloseAndFlush();

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

    }
}