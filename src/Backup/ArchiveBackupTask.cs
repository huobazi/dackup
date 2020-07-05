using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.Logging;

namespace Dackup.Backup
{
    public class ArchiveBackupTask : BackupTaskBase
    {
        private readonly ILogger logger;
        public string Name { get; set; }
        public List<string> IncludePathList { get; set; }
        public List<string> ExcludePathList { get; set; }
        private ArchiveBackupTask() { }

        public ArchiveBackupTask(ILogger<ArchiveBackupTask> logger) => this.logger = logger;
        protected override ILogger Logger
        {
            get
            {
                return this.logger;
            }
        }
        protected override BackupTaskResult Backup()
        {
            if (IncludePathList == null || IncludePathList.Count <= 0)
            {
                return new BackupTaskResult
                {
                    Result = false,
                    Message = "No archives files setting"
                };
            }

            logger.LogInformation($"Archive {Name} backup start");

            var directory = Path.Combine(DackupContext.Current.TmpPath, "archives");
            Directory.CreateDirectory(directory);

            this.IncludePathList.ForEach(file =>
            {
                logger.LogInformation($"Archive {Name} backup : {file}");

                FileAttributes attr = File.GetAttributes(file);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    var destDirectory = Path.Combine(directory, file.TrimStart('/'));
                    Utils.DirectoryCopy(file, destDirectory, ExcludePathList);
                }
                else
                {
                    var destFile      = Path.Combine(directory, file.TrimStart('/'));
                    var destDirectory = destFile.Substring(0, destFile.LastIndexOf('/') + 1);
                    Directory.CreateDirectory(destDirectory);
                    Utils.FileCopy(file, destFile, ExcludePathList);
                }
            });

            var tgzFileName = Path.Combine(Path.Combine(DackupContext.Current.TmpPath, $"archives_{this.Name}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.tar.gz"));

            Utils.CreateTarGZ(tgzFileName, directory);

            return new BackupTaskResult
            {
                Result    = true,
                FilesList = new List<string> { tgzFileName },
            };
        }
    }
}