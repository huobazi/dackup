using System;
using System.Collections.Generic;


namespace dackup
{
    public interface IBackupTask
    {
        BackupTaskResult Backup();
    }

    public class BackupTaskResult
    {
        public bool Result { get; set; }
        public List<string> FilesList { get; set; }
        public string Message { get; set; }
    }
}