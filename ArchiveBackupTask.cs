
using System;
using System.Collections.Generic;

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
            return new BackupTaskResult
            {
                Result = true,
            };

        }
    }
}