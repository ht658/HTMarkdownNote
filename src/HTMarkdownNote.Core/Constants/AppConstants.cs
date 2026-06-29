using System;
using System.IO;

namespace HTMarkdownNote.Core.Constants;

public static class AppConstants
{
    public const string AppName = "HTMarkdownNote";
    public const string AppVersion = "1.0.0";
    
    // 文件与目录
    public const string MetadataFileName = "metadata.json";
    public const string NotesDirectoryName = "notes";
    public const string BackupsDirectoryName = "Backups";
    public const string RecycleDirectoryName = "Recycle";
    
    public static readonly string AppDataPath = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);
    
    public static readonly string NotesPath = Path.Combine(AppDataPath, NotesDirectoryName);
    public static readonly string BackupsPath = Path.Combine(AppDataPath, BackupsDirectoryName);
    public static readonly string RecyclePath = Path.Combine(AppDataPath, RecycleDirectoryName);
    public static readonly string MetadataPath = Path.Combine(AppDataPath, MetadataFileName);
    
    // 自动保存延迟（毫秒）
    public const int AutoSaveDelayMs = 300;
    
    // 回收站保留天数
    public const int RecycleBinRetentionDays = 30;
    
    // 备份保留天数
    public const int BackupRetentionDays = 7;
    
    // 性能指标
    public const int ColdStartTargetMs = 700;
    public const int HotStartTargetMs = 200;
    public const int MaxIdleMemoryMb = 80;
    public const int MaxIdleMemoryHardLimitMb = 100;
}