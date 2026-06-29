using System;

namespace HTMarkdownNote.Core.Models;

public class Note
{
    /// <summary>
    /// 便签唯一标识
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// 对应的 .md 文件名
    /// </summary>
    public string Filename { get; set; } = string.Empty;
    
    /// <summary>
    /// 便签标题（首行内容缓存）
    /// </summary>
    public string Title { get; set; } = "未命名便签";
    
    /// <summary>
    /// 便签内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime Modified { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 编辑窗口 X 坐标
    /// </summary>
    public int WindowX { get; set; } = 100;
    
    /// <summary>
    /// 编辑窗口 Y 坐标
    /// </summary>
    public int WindowY { get; set; } = 100;
    
    /// <summary>
    /// 编辑窗口宽度
    /// </summary>
    public int WindowWidth { get; set; } = 800;
    
    /// <summary>
    /// 编辑窗口高度
    /// </summary>
    public int WindowHeight { get; set; } = 600;
    
    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsTopmost { get; set; } = false;
}