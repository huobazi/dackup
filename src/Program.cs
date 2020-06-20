using System;
using System.IO;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

using Serilog;
using McMaster.Extensions.CommandLineUtils;

using dackup.Configuration;

namespace dackup
{
    class Program
    {
        public static int Main(string[] args)
        {
            var statistics           = new Statistics();
                statistics.StartedAt = DateTime.Now;

            var app = new CommandLineApplication
            {
                Name        = "dackup",
                Description = "A backup app for your server or database or desktop",
            };

            app.HelpOption(inherited: true);

            app.Command("new", newCmd =>
            {
                newCmd.Description = "Generate a config file";

                var modelName = newCmd.Argument("model", "Name of the model").IsRequired();

                newCmd.OnExecute(() =>
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
                    try
                    {
                        var logPath = logPathConfig.Value();
                        var tmpPath = tmpPathConfig.Value();
                        
                        var configFilePath = configFile.Value();

                        var performConfig = ApplicationHelper.PrepaireConfig(configFilePath,logPath,tmpPath);

                        statistics.ModelName = performConfig.Name;

                        Directory.CreateDirectory(DackupContext.Current.TmpPath);

                        // run backup
                        var backupTasks = ApplicationHelper.RunBackup(performConfig);
                        backupTasks.Wait();
                        
                        statistics.FinishedAt = DateTime.Now;

                        // run store
                        var (storageUploadTasks, storagePurgeTasks) = ApplicationHelper.RunStorage(performConfig);
                        var storageTasks                            = Task.WhenAll(storageUploadTasks, storagePurgeTasks);

                        // run notify                     
                        var notifyTasks = ApplicationHelper.RunNotify(performConfig, statistics);

                        // wait
                        storageTasks.Wait();

                        ApplicationHelper.Clean();
                        
                        notifyTasks.Wait();

                        Log.Information("Dackup done ");

                    }
                    catch (Exception exception)
                    {
                        Log.Error(Utils.FlattenException(exception));
                    }
                    finally
                    {
                        Log.CloseAndFlush();
                    }
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