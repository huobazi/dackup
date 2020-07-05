
using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Dackup.Backup
{
    public abstract class DatabaseBackupTask : BackupTaskBase
    {
        private string dbType;
        protected override ILogger Logger
        {
            get;
        }
        public Dictionary<string, string> CommandOptions { get; private set; }
        public DatabaseBackupTask(string dbType)
        {
            this.dbType         = dbType;
            this.CommandOptions = new Dictionary<string, string>();
        }
        protected abstract bool CheckDbConnection();
        protected abstract bool CheckDbBackupCommand();
        protected abstract BackupTaskResult CreateNewBackup();
        protected override BackupTaskResult Backup()
        {
            if (CheckDbConnection() && CheckDbBackupCommand())
            {
                return CreateNewBackup();
            }
            else
            {
                return new BackupTaskResult
                {
                    Result = false,
                };
            }
        }
        public void AddCommandOptions(string key, string value)
        {
            if (CommandOptions.ContainsKey(key))
            {
                CommandOptions[key] = value;
            }
            else
            {
                this.CommandOptions.Add(key, value);
            }
        }
        protected void RemoveCommandOptions(string key)
        {
            if (CommandOptions.ContainsKey(key))
            {
                CommandOptions.Remove(key);
            }
        }
    }
}