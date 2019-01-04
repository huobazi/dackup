
using System;
using System.Collections.Generic;

using Serilog;

namespace dackup
{
    public abstract class DatabaseBackupTask : IBackupTask
    {
        private string dbType;
        public DatabaseBackupTask(string dbType)
        {
            this.dbType = dbType;
        }

        public abstract void CheckDbConnection();

        public abstract BackupTaskResult CreateNewBackup();

        public BackupTaskResult Backup()
        {
            CheckDbConnection();
            var result = CreateNewBackup();
            return result;
        }
    }
}