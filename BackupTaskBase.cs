
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Serilog;

namespace dackup
{
    public abstract class BackupTaskBase : IBackupTask
    {
        public async Task<BackupTaskResult> BackupAsync()
        {
            Log.Information($"======== Dackup start [{this.GetType().Name }.BackupAsync] ========");

            var task = Task.Run(() =>
            {
                var result = Backup();
                if (result.Result)
                {
                    BackupContext.Current.AddToGenerateFilesList(result.FilesList);
                }
                return result;
            });
            return await task;
        }

        protected abstract BackupTaskResult Backup();
    }
}