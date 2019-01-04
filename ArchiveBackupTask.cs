
using System;
using System.Collections.Generic;

using Serilog;

namespace dackup
{
    public class ArchiveBackupTask : IBackupTask
    {
        private List<string> pathList;
        public ArchiveBackupTask(List<string> pathList)
        {
            this.pathList = pathList;
        }
        public BackupTaskResult Backup()
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