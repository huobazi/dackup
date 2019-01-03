
using System;
using System.Collections.Generic;

public class ArchiveBackupTask : IBackupTask
{
    private List<string> pathList;
    public ArchiveBackupTask(List<string> pathList)
    {
        this.pathList = pathList;
    }
    public BackupTaskResult Run()
    {
        return new BackupTaskResult
        {
            Result = true,
        };

    }
}