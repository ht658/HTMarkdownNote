using System;
using System.Collections.Generic;

namespace HTMarkdownNote.Core.Models;

public class Metadata
{
    /// <summary>
    /// 元数据版本
    /// </summary>
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 所有便签
    /// </summary>
    public List<NoteMetadata> Notes { get; set; } = new();
    
    /// <summary>
    /// 回收站中的便签
    /// </summary>
    public List<DeletedNoteMetadata> DeletedNotes { get; set; } = new();
    
    /// <summary>
    /// 应用设置
    /// </summary>
    public AppSettings Settings { get; set; } = new();
    
    /// <summary>
    /// 会话状态
    /// </summary>
    public SessionState SessionState { get; set; } = new();
}

public class NoteMetadata
{
    public Guid Id { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public int WindowX { get; set; }
    public int WindowY { get; set; }
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
    public bool IsTopmost { get; set; }
}

public class DeletedNoteMetadata
{
    public Guid Id { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime Deleted { get; set; }
    public DateTime ExpireAt { get; set; }
}

public class AppSettings
{
    public string SortBy { get; set; } = "modified";
    public string SortOrder { get; set; } = "desc";
    public string Theme { get; set; } = "system";
    public string FontFamily { get; set; } = "Segoe UI";
    public int FontSize { get; set; } = 12;
    public bool ShowModifiedTime { get; set; } = true;
}

public class SessionState
{
    public DateTime LastExitTime { get; set; } = DateTime.UtcNow;
    public bool IsNormalExit { get; set; } = true;
    public List<Guid> OpenWindowIds { get; set; } = new();
}