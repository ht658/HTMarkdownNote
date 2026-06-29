using System;
using HTMarkdownNote.Core.Constants;

namespace HTMarkdownNote.Core.Utils;

/// <summary>
/// 路径辅助工具
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// 根据便签 ID 生成文件名
    /// </summary>
    public static string GetNoteFilename(Guid noteId)
    {
        return $"note-{noteId:n}.md";
    }

    /// <summary>
    /// 根据便签 ID 获取便签文件完整路径
    /// </summary>
    public static string GetNoteFilePath(Guid noteId)
    {
        return Path.Combine(AppConstants.NotesPath, GetNoteFilename(noteId));
    }

    /// <summary>
    /// 根据便签 ID 获取回收站文件路径
    /// </summary>
    public static string GetRecycledNoteFilePath(Guid noteId)
    {
        return Path.Combine(AppConstants.RecyclePath, GetNoteFilename(noteId));
    }

    /// <summary>
    /// 生成备份文件名
    /// </summary>
    public static string GetBackupFilename(DateTime date)
    {
        return $"backup-{date:yyyy-MM-dd}.zip";
    }

    /// <summary>
    /// 获取备份文件完整路径
    /// </summary>
    public static string GetBackupFilePath(DateTime date)
    {
        return Path.Combine(AppConstants.BackupsPath, GetBackupFilename(date));
    }
}