using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace dackup
{
    public class BackupTaskFactory
    {
        private readonly ILogger logger;

        public BackupTaskFactory(ILogger<BackupTaskFactory> logger)
        {
            this.logger = logger;
        }

        public ArchiveBackupTask CreateArchiveBackupTask(string name, List<string> includePathList, List<string> excludePathList)
        {
            return new ArchiveBackupTask(this.logger, name, includePathList, excludePathList);
        }
    }
}