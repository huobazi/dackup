using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Extensions.Logging;

namespace dackup
{
    public class ArchiveBackupTask : BackupTaskBase
    {
        private readonly ILogger logger;
        private readonly string name;
        private readonly List<string> includePathList;
        private readonly List<string> excludePathList;
        private ArchiveBackupTask() { }

        public ArchiveBackupTask(ILogger logger, string name, List<string> includePathList, List<string> excludePathList)
        {
            this.logger          = logger;
            this.name            = name;
            this.includePathList = includePathList;
            this.excludePathList = excludePathList;
        }
        protected override ILogger Logger
        {
            get
            {
                return this.logger;
            }
        }
        protected override BackupTaskResult Backup()
        {
            if (includePathList == null || includePathList.Count <= 0)
            {
                return new BackupTaskResult
                {
                    Result = false,
                    Message = "No archives files setting"
                };
            }

            logger.LogInformation($"Archive backup start");

            var directory = Path.Combine(DackupContext.Current.TmpPath, "archives");
            Directory.CreateDirectory(directory);

            this.includePathList.ForEach(file =>
            {
                logger.LogInformation($"Archive backup : {file}");

                FileAttributes attr = File.GetAttributes(file);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    var destDirectory = Path.Combine(directory, file.TrimStart('/'));
                    Utils.DirectoryCopy(file, destDirectory, excludePathList);
                }
                else
                {
                    var destFile = Path.Combine(directory, file.TrimStart('/'));
                    var destDirectory = destFile.Substring(0, destFile.LastIndexOf('/') + 1);
                    Directory.CreateDirectory(destDirectory);
                    Utils.FileCopy(file, destFile, excludePathList);
                }
            });

            var tgzFileName = Path.Combine(Path.Combine(DackupContext.Current.TmpPath, $"archives_{this.name}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.tar.gz"));

            Utils.CreateTarGZ(tgzFileName, directory);

            return new BackupTaskResult
            {
                Result = true,
                FilesList = new List<string> { tgzFileName },
            };
        }
    }
}