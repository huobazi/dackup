using System;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace Dackup.Backup
{
    public interface IBackupTask
    {
        Task<BackupTaskResult> BackupAsync();
    }

    public class BackupTaskResult
    {
        public bool Result { get; set; }
        public List<string> FilesList { get; set; }
        public string Message { get; set; }
    }
}