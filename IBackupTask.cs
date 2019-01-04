using System;
using System.Collections.Generic;

public interface IBackupTask
{
    BackupTaskResult Run();
}

public class BackupTaskResult
{
    public bool Result { get; set; }
    public List<string> FilesList{get;set;}
    public string Message { get; set; }
}
