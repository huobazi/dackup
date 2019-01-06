
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Serilog;

namespace dackup
{
    public class ArchiveBackupTask : BackupTaskBase
    {
        private List<string> pathList;
        public ArchiveBackupTask(List<string> pathList)
        {
            this.pathList = pathList;
        }

        protected override BackupTaskResult Backup()
        {
            if (pathList == null || pathList.Count <= 0)
            {
                return new BackupTaskResult
                {
                    Result = false, Message= "No archives files setting"
                };
            }
            Log.Information($"Archive backup start");
            this.pathList.ForEach(file =>
            {
                Log.Information($"Archive backup ===> : {file}");

            });
            return new BackupTaskResult
            {
                Result = true,
            };

        }
    }
}