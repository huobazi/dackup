// using System;
// using System.IO;
// using System.Linq;
// using System.Collections.Specialized;
// using System.Collections.Generic;
// using System.Threading.Tasks;

// using Serilog;

// using dackup.Configuration;

// namespace dackup
// {
//     public class DackupApplication
//     {
//         private readonly string configFilePath;
//         private readonly string logPath;
//         private readonly string tmpPath;
//         public DackupApplication(string configFilePath, string logPath, string tmpPath)
//         {
//             this.configFilePath = configFilePath;
//             this.logPath        = logPath;
//             this.tmpPath        = tmpPath;
//         }
//         public async Task Run()
//         {      
//             var statistics           = new Statistics();
//                 statistics.StartedAt = DateTime.Now;
//             var performConfig        = PrepaireConfig(configFilePath,logPath,tmpPath);
//                 statistics.ModelName = performConfig.Name;

//             Directory.CreateDirectory(DackupContext.Current.TmpPath);

//             // run backup
//             var backupTasks = RunBackup(performConfig);
//             await backupTasks;
            
//             statistics.FinishedAt = DateTime.Now;

//             // run store
//             var (storageUploadTasks, storagePurgeTasks) = RunStorage(performConfig);

//             // run notify                     
//             var notifyTasks = RunNotify(performConfig, statistics);

//             // wait
//             await storageUploadTasks;
//             await storagePurgeTasks;

//             Clean();
            
//             await notifyTasks;

//             Log.Information("Dackup done ");
//         }
//         private PerformConfig PrepaireConfig(string configfile, string logPath, string tmpPath)
//         {
//             if (string.IsNullOrEmpty(logPath))
//             {
//                 logPath = "log";
//             }
//             if (string.IsNullOrEmpty(tmpPath))
//             {
//                 tmpPath = "tmp";
//             }

//             var tmpWorkDirPath = Path.Combine(tmpPath, $"dackup-tmp-{DateTime.UtcNow:s}");
//             DackupContext.Create(Path.Join(logPath, "dackup.log"), tmpWorkDirPath);

//             Log.Logger = new LoggerConfiguration()
//             .MinimumLevel.Information()
//             .WriteTo.Console()
//             .WriteTo.File(DackupContext.Current.LogFile, rollingInterval: RollingInterval.Month, rollOnFileSizeLimit: true)
//             .CreateLogger();

//             AppDomain.CurrentDomain.UnhandledException += (s, e) => Log.Error("*** Crash! ***", "UnhandledException");
//             TaskScheduler.UnobservedTaskException += (s, e) => Log.Error("*** Crash! ***", "UnobservedTaskException");

//             return PerformConfigHelper.LoadFrom(configfile);

//         }
//         public Task<BackupTaskResult[]> RunBackup(PerformConfig cfg)
//         {
//             Log.Information("Dackup start backup task ");

//             var backupTaskList = PerformConfigHelper.ParseBackupTaskFromConfig(cfg);
//             var backupTaskResult = new List<Task<BackupTaskResult>>();
//             backupTaskList.ForEach(task =>
//             {
//                 backupTaskResult.Add(task.BackupAsync());
//             });
//             return Task.WhenAll(backupTaskResult.ToArray());
//         }
//         public (Task<UploadResult[]>, Task<PurgeResult[]>) RunStorage(PerformConfig cfg)
//         {
//             Log.Information("Dackup start storage task ");

//             var storageList = PerformConfigHelper.ParseStorageFromConfig(cfg);
//             var storageUploadResultList = new List<Task<UploadResult>>();
//             var storagePurgeResultList = new List<Task<PurgeResult>>();

//             storageList.ForEach(storage =>
//             {
//                 DackupContext.Current.GenerateFilesList.ForEach(file =>
//                 {
//                     storageUploadResultList.Add(storage.UploadAsync(file));
//                 });
//                 storagePurgeResultList.Add(storage.PurgeAsync());
//             });

//             var storageUploadTasks = Task.WhenAll(storageUploadResultList.ToArray());
//             var storagePurgeTasks = Task.WhenAll(storagePurgeResultList.ToArray());

//             return (storageUploadTasks, storagePurgeTasks);
//         }
//         public Task<NotifyResult[]> RunNotify(PerformConfig cfg, Statistics statistics)
//         {
//             Log.Information("Dackup start notify task ");

//             var notifyList = PerformConfigHelper.ParseNotifyFromConfig(cfg);

//             var notifyResultList = new List<Task<NotifyResult>>();

//             notifyList.ForEach(notify =>
//             {
//                 notifyResultList.Add(notify.NotifyAsync(statistics));
//             });

//             return Task.WhenAll(notifyResultList.ToArray());
//         }
//         public void Clean()
//         {
//             Log.Information("Dackup clean tmp folder ");

//             var di = new DirectoryInfo(DackupContext.Current.TmpPath);
//             foreach (var file in di.GetFiles())
//             {
//                 file.Delete();
//             }
//             foreach (var dir in di.GetDirectories())
//             {
//                 dir.Delete(true);
//             }

//             Directory.Delete(DackupContext.Current.TmpPath);

//         }
//     }
// }