
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Serilog;

namespace dackup
{
    public class ArchiveBackupTask : BackupTaskBase
    {
        private List<string> includePathList;
        private List<string> excludePathList;

        private ArchiveBackupTask(){}
        
        public ArchiveBackupTask(List<string> includePathList, List<string> excludePathList)
        {
            this.includePathList = includePathList;
            this.excludePathList = excludePathList;
        }

        protected override BackupTaskResult Backup()
        {
            if (includePathList == null || includePathList.Count <= 0)
            {
                return new BackupTaskResult
                {
                    Result = false, Message= "No archives files setting"
                };
            }
            Log.Information($"Archive backup start");
            this.includePathList.ForEach(file =>
            {
                Log.Information($"Archive backup ===> : {file}");
                // TODO archive

            });
            return new BackupTaskResult
            {
                Result = true,
            };

        }
    }
}