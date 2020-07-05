using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Dackup.Backup
{
    public abstract class BackupTaskBase : IBackupTask
    {
        protected abstract ILogger Logger
        {
            get;
        }
        public async Task<BackupTaskResult> BackupAsync()
        {
            Logger.LogInformation($"Dackup start [{this.GetType().Name }.BackupAsync]");

            var task = Task.Run(() =>
            {
                var result = Backup();
                if (result.Result)
                {
                    DackupContext.Current.AddToGenerateFilesList(result.FilesList);
                }
                return result;
            });
            return await task;
        }

        protected abstract BackupTaskResult Backup();
    }
}