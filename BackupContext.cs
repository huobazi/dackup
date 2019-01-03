
public sealed class BackupContext
{
    public static readonly BackupContext instance;

    static BackupContext()
    {
        instance = new BackupContext();
    }

    public static BackupContext Current {
        get
        {
            return instance;
        }
    }

    public string LogFile{get;set;}

    public string TmpPath{get;set;}
}