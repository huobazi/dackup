using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

using Serilog;

namespace dackup
{
    public class ArchiveBackupTask: BackupTaskBase
    {
        private string name;
        private List<string> includePathList;
        private List<string> excludePathList;
        private ArchiveBackupTask() { }

        public ArchiveBackupTask(string name, List<string> includePathList, List<string> excludePathList)
        {
            this.name            = name;
            this.includePathList = includePathList;
            this.excludePathList = excludePathList;
        }
        protected override BackupTaskResult Backup()
        {
            if (includePathList == null || includePathList.Count <= 0)
            {
                return new BackupTaskResult
                {
                    Result  = false,
                    Message = "No archives files setting"
                };
            }

            Log.Information($"Archive backup start");

            var directory = Path.Combine(DackupContext.Current.TmpPath, "archives");
            Directory.CreateDirectory(directory);

            this.includePathList.ForEach(file =>
            {
                Log.Information($"Archive backup : {file}");

                FileAttributes attr = File.GetAttributes(file);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    var destDirectory = Path.Combine(directory, file.TrimStart('/'));
                    Utils.DirectoryCopy(file, destDirectory, excludePathList);
                }
                else
                {
                    var destFile      = Path.Combine(directory, file.TrimStart('/'));
                    var destDirectory = destFile.Substring(0, destFile.LastIndexOf('/') + 1);
                    Directory.CreateDirectory(destDirectory);
                    Utils.FileCopy(file, destFile, excludePathList);
                }
            });

            var tgzFileName = Path.Combine(Path.Combine(DackupContext.Current.TmpPath, $"archives_{this.name}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.tar.gz"));

            Utils.CreateTarGZ(tgzFileName, directory);

            return new BackupTaskResult
            {
                Result    = true,
                FilesList = new List<string> { tgzFileName },
            };
        }
    }
}