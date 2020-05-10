
using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using Serilog;

namespace dackup
{
    public abstract class DatabaseBackupTask: BackupTaskBase
    {
        private string dbType;
        public Dictionary<string, string> CommandOptions { get; private set; }
        public DatabaseBackupTask(string dbType)
        {
            this.dbType         = dbType;
            this.CommandOptions = new Dictionary<string, string>();
        }
        public abstract void CheckDbConnection();
        public abstract BackupTaskResult CreateNewBackup();
        protected override BackupTaskResult Backup()
        {
            CheckDbConnection();
            var result = CreateNewBackup();
            return result;
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
        public void RemoveCommandOptions(string key)
        {
            if (CommandOptions.ContainsKey(key))
            {
                CommandOptions.Remove(key);
            }
        }
    }
}