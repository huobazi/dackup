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
            var startAt = DateTime.Now;

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
                    try
                    {
                        var logPath = logPathConfig.Value();
                        var tmpPath = tmpPathConfig.Value();
                        
                        var configFilePath = configFile.Value();

                        var performConfig = ApplicationHelper.PrepaireConfig(configFilePath,logPath,tmpPath);

                        Directory.CreateDirectory(DackupContext.Current.TmpPath);

                        // run backup
                        var backupTasks = ApplicationHelper.RunBackup(performConfig);
                        backupTasks.Wait();

                        // run store
                        var storageTask = ApplicationHelper.RunStorage(performConfig);
                        var storageTasks = Task.WhenAll(storageTask.Item1, storageTask.Item2);

                        // run notify
                        var now = DateTime.Now;
                        var duration = (now - startAt);

                        var sb = new StringBuilder();
                        sb.AppendLine($"Backup Completed Successfully!");
                        sb.AppendLine($"Model={performConfig.Name}");
                        sb.AppendLine($"Start={startAt}");
                        sb.AppendLine($"Finished={now}");
                        sb.AppendLine($"Duration={duration}");

                        var notifyTasks = ApplicationHelper.RunNotify(performConfig, sb.ToString());

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