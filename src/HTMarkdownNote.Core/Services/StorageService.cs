using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HTMarkdownNote.Core.Models;
using HTMarkdownNote.Core.Constants;

namespace HTMarkdownNote.Core.Services;

/// <summary>
/// 存储服务接口
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// 初始化存储服务
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 加载元数据
    /// </summary>
    Task<Metadata> LoadMetadataAsync();

    /// <summary>
    /// 保存元数据
    /// </summary>
    Task SaveMetadataAsync(Metadata metadata);

    /// <summary>
    /// 读取便签文件内容
    /// </summary>
    Task<string> ReadNoteFileAsync(Guid noteId);

    /// <summary>
    /// 写入便签文件（原子操作）
    /// </summary>
    Task WriteNoteFileAsync(Guid noteId, string content);

    /// <summary>
    /// 删除便签文件
    /// </summary>
    Task DeleteNoteFileAsync(Guid noteId);

    /// <summary>
    /// 移动文件到回收站
    /// </summary>
    Task MoveToRecycleAsync(Guid noteId);

    /// <summary>
    /// 从回收站恢复文件
    /// </summary>
    Task RestoreFromRecycleAsync(Guid noteId);

    /// <summary>
    /// 永久删除回收站中的文件
    /// </summary>
    Task PermanentlyDeleteFromRecycleAsync(Guid noteId);

    /// <summary>
    /// 清理过期的回收站文件
    /// </summary>
    Task CleanupExpiredRecycledNotesAsync();

    /// <summary>
    /// 检测并恢复异常退出
    /// </summary>
    Task DetectAndRecoverFromCrashAsync();
}

/// <summary>
/// 存储服务实现
/// </summary>
public class StorageService : IStorageService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task InitializeAsync()
    {
        // 初始化应用数据目录
        Utils.AppDataInitializer.Initialize();

        // 如果元数据不存在，创建空的元数据文件
        if (!File.Exists(AppConstants.MetadataPath))
        {
            var emptyMetadata = new Metadata();
            await SaveMetadataAsync(emptyMetadata);
        }

        // 清理过期的回收站文件
        await CleanupExpiredRecycledNotesAsync();
    }

    public async Task<Metadata> LoadMetadataAsync()
    {
        try
        {
            if (!File.Exists(AppConstants.MetadataPath))
            {
                return new Metadata();
            }

            var json = await File.ReadAllTextAsync(AppConstants.MetadataPath);
            var metadata = JsonSerializer.Deserialize<Metadata>(json, _jsonOptions);
            return metadata ?? new Metadata();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load metadata: {ex.Message}", ex);
        }
    }

    public async Task SaveMetadataAsync(Metadata metadata)
    {
        try
        {
            metadata.LastModified = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(metadata, _jsonOptions);
            Utils.AtomicFileWriter.WriteAllText(AppConstants.MetadataPath, json);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save metadata: {ex.Message}", ex);
        }
    }

    public async Task<string> ReadNoteFileAsync(Guid noteId)
    {
        try
        {
            var filePath = Utils.PathHelper.GetNoteFilePath(noteId);
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            return await File.ReadAllTextAsync(filePath, System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read note file: {ex.Message}", ex);
        }
    }

    public async Task WriteNoteFileAsync(Guid noteId, string content)
    {
        try
        {
            var filePath = Utils.PathHelper.GetNoteFilePath(noteId);
            Utils.AtomicFileWriter.WriteAllText(filePath, content);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to write note file: {ex.Message}", ex);
        }
    }

    public async Task DeleteNoteFileAsync(Guid noteId)
    {
        try
        {
            var filePath = Utils.PathHelper.GetNoteFilePath(noteId);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete note file: {ex.Message}", ex);
        }
    }

    public async Task MoveToRecycleAsync(Guid noteId)
    {
        try
        {
            var sourcePath = Utils.PathHelper.GetNoteFilePath(noteId);
            var targetPath = Utils.PathHelper.GetRecycledNoteFilePath(noteId);

            if (File.Exists(sourcePath))
            {
                File.Move(sourcePath, targetPath, overwrite: true);
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to move note to recycle: {ex.Message}", ex);
        }
    }

    public async Task RestoreFromRecycleAsync(Guid noteId)
    {
        try
        {
            var sourcePath = Utils.PathHelper.GetRecycledNoteFilePath(noteId);
            var targetPath = Utils.PathHelper.GetNoteFilePath(noteId);

            if (File.Exists(sourcePath))
            {
                File.Move(sourcePath, targetPath, overwrite: true);
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to restore note from recycle: {ex.Message}", ex);
        }
    }

    public async Task PermanentlyDeleteFromRecycleAsync(Guid noteId)
    {
        try
        {
            var filePath = Utils.PathHelper.GetRecycledNoteFilePath(noteId);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to permanently delete note: {ex.Message}", ex);
        }
    }

    public async Task CleanupExpiredRecycledNotesAsync()
    {
        try
        {
            var metadata = await LoadMetadataAsync();
            var now = DateTime.UtcNow;
            var expiredNotes = metadata.DeletedNotes.Where(n => n.ExpireAt < now).ToList();

            foreach (var expiredNote in expiredNotes)
            {
                await PermanentlyDeleteFromRecycleAsync(expiredNote.Id);
                metadata.DeletedNotes.Remove(expiredNote);
            }

            if (expiredNotes.Count > 0)
            {
                await SaveMetadataAsync(metadata);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to cleanup expired recycle notes: {ex.Message}", ex);
        }
    }

    public async Task DetectAndRecoverFromCrashAsync()
    {
        try
        {
            var metadata = await LoadMetadataAsync();
            
            // 标记为正常退出（在应用正常关闭时会重新设置为 true）
            if (!metadata.SessionState.IsNormalExit)
            {
                // 记录日志或进行恢复逻辑
                // 当前仅标记为已检测，UI 层会根据此进行恢复
            }

            // 更新当前会话状态为异常状态（待正常关闭时更新）
            metadata.SessionState.LastExitTime = DateTime.UtcNow;
            metadata.SessionState.IsNormalExit = false;
            await SaveMetadataAsync(metadata);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to detect and recover from crash: {ex.Message}", ex);
        }
    }
}