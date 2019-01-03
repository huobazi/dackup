
using System;
using System.Collections.Generic;
public class DatabaseBackupTask : IBackupTask
{
    private string dbType; 
    public DatabaseBackupTask(string dbType)
    {
        this.dbType = dbType;
    }
    public BackupTaskResult Run( )
    {
        return new BackupTaskResult
        {
            Result = true,
        };

    }
}