
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Serilog;

namespace dackup
{
    public abstract class DatabaseBackupTask : BackupTaskBase
    {
        private string dbType;
        public DatabaseBackupTask(string dbType)
        {
            this.dbType = dbType;
        }

        public abstract void CheckDbConnection();

        public abstract BackupTaskResult CreateNewBackup();

        protected override BackupTaskResult Backup()
        {
            CheckDbConnection();
            var result = CreateNewBackup();
            return result;
        }
    }
}