using HTMarkdownNote.Core.Models;
using HTMarkdownNote.Core.Constants;

namespace HTMarkdownNote.Core.Services;

/// <summary>
/// 便签服务接口
/// </summary>
public interface INoteService
{
    /// <summary>
    /// 创建新便签
    /// </summary>
    Task<Note> CreateNoteAsync();

    /// <summary>
    /// 获取所有便签
    /// </summary>
    Task<IEnumerable<Note>> GetAllNotesAsync();

    /// <summary>
    /// 获取单个便签
    /// </summary>
    Task<Note?> GetNoteAsync(Guid noteId);

    /// <summary>
    /// 更新便签
    /// </summary>
    Task UpdateNoteAsync(Note note);

    /// <summary>
    /// 删除便签（移到回收站）
    /// </summary>
    Task DeleteNoteAsync(Guid noteId);

    /// <summary>
    /// 搜索便签
    /// </summary>
    Task<IEnumerable<Note>> SearchNotesAsync(string keyword);

    /// <summary>
    /// 获取排序后的便签
    /// </summary>
    Task<IEnumerable<Note>> GetSortedNotesAsync(string sortBy = "modified", string sortOrder = "desc");

    /// <summary>
    /// 获取回收站中的便签
    /// </summary>
    Task<IEnumerable<Note>> GetRecycledNotesAsync();

    /// <summary>
    /// 恢复便签
    /// </summary>
    Task RestoreNoteAsync(Guid noteId);

    /// <summary>
    /// 永久删除便签
    /// </summary>
    Task PermanentlyDeleteNoteAsync(Guid noteId);

    /// <summary>
    /// 清空回收站
    /// </summary>
    Task ClearRecycleBinAsync();

    /// <summary>
    /// 切换便签置顶状态
    /// </summary>
    Task ToggleToppmostAsync(Guid noteId);
}

/// <summary>
/// 便签服务实现
/// </summary>
public class NoteService : INoteService
{
    private readonly IStorageService _storageService;
    private Metadata? _metadata;
    private List<Note> _notesCache = new();

    public NoteService(IStorageService storageService)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    /// <summary>
    /// 初始化服务，加载元数据
    /// </summary>
    public async Task InitializeAsync()
    {
        await _storageService.InitializeAsync();
        _metadata = await _storageService.LoadMetadataAsync();
        await LoadNotesFromMetadataAsync();
    }

    public async Task<Note> CreateNoteAsync()
    {
        var noteId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        
        var note = new Note
        {
            Id = noteId,
            Filename = Utils.PathHelper.GetNoteFilename(noteId),
            Created = now,
            Modified = now
        };

        // 添加到元数据
        var metadata = await _storageService.LoadMetadataAsync();
        metadata.Notes.Add(new NoteMetadata
        {
            Id = noteId,
            Filename = note.Filename,
            Title = note.Title,
            Created = now,
            Modified = now,
            WindowX = note.WindowX,
            WindowY = note.WindowY,
            WindowWidth = note.WindowWidth,
            WindowHeight = note.WindowHeight,
            IsTopmost = note.IsTopmost
        });

        await _storageService.SaveMetadataAsync(metadata);
        _notesCache.Add(note);
        
        return note;
    }

    public async Task<IEnumerable<Note>> GetAllNotesAsync()
    {
        if (_notesCache.Count == 0)
        {
            await LoadNotesFromMetadataAsync();
        }
        return _notesCache;
    }

    public async Task<Note?> GetNoteAsync(Guid noteId)
    {
        var note = _notesCache.FirstOrDefault(n => n.Id == noteId);
        if (note != null)
        {
            // 从文件重新加载最新内容
            note.Content = await _storageService.ReadNoteFileAsync(noteId);
        }
        return note;
    }

    public async Task UpdateNoteAsync(Note note)
    {
        // 更新文件内容
        await _storageService.WriteNoteFileAsync(note.Id, note.Content);

        // 更新元数据
        var metadata = await _storageService.LoadMetadataAsync();
        var noteMetadata = metadata.Notes.FirstOrDefault(n => n.Id == note.Id);
        if (noteMetadata != null)
        {
            noteMetadata.Title = note.Title;
            noteMetadata.Modified = DateTime.UtcNow;
            noteMetadata.WindowX = note.WindowX;
            noteMetadata.WindowY = note.WindowY;
            noteMetadata.WindowWidth = note.WindowWidth;
            noteMetadata.WindowHeight = note.WindowHeight;
            noteMetadata.IsTopmost = note.IsTopmost;
        }

        await _storageService.SaveMetadataAsync(metadata);

        // 更新缓存
        var cachedNote = _notesCache.FirstOrDefault(n => n.Id == note.Id);
        if (cachedNote != null)
        {
            cachedNote.Title = note.Title;
            cachedNote.Modified = DateTime.UtcNow;
            cachedNote.WindowX = note.WindowX;
            cachedNote.WindowY = note.WindowY;
            cachedNote.WindowWidth = note.WindowWidth;
            cachedNote.WindowHeight = note.WindowHeight;
            cachedNote.IsTopmost = note.IsTopmost;
        }
    }

    public async Task DeleteNoteAsync(Guid noteId)
    {
        var metadata = await _storageService.LoadMetadataAsync();
        var noteMetadata = metadata.Notes.FirstOrDefault(n => n.Id == noteId);
        
        if (noteMetadata != null)
        {
            // 移到回收站
            await _storageService.MoveToRecycleAsync(noteId);

            // 更新元数据
            metadata.Notes.Remove(noteMetadata);
            metadata.DeletedNotes.Add(new DeletedNoteMetadata
            {
                Id = noteId,
                Filename = noteMetadata.Filename,
                Title = noteMetadata.Title,
                Deleted = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddDays(AppConstants.RecycleBinRetentionDays)
            });

            await _storageService.SaveMetadataAsync(metadata);

            // 从缓存移除
            var cachedNote = _notesCache.FirstOrDefault(n => n.Id == noteId);
            if (cachedNote != null)
            {
                _notesCache.Remove(cachedNote);
            }
        }
    }

    public async Task<IEnumerable<Note>> SearchNotesAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return await GetAllNotesAsync();
        }

        var results = new List<Note>();
        foreach (var note in _notesCache)
        {
            if (note.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                note.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(note);
            }
        }

        return await Task.FromResult(results);
    }

    public async Task<IEnumerable<Note>> GetSortedNotesAsync(string sortBy = "modified", string sortOrder = "desc")
    {
        var notes = await GetAllNotesAsync();
        
        var sorted = sortBy.ToLower() switch
        {
            "created" => sortOrder.ToLower() == "desc" 
                ? notes.OrderByDescending(n => n.Created)
                : notes.OrderBy(n => n.Created),
            _ => sortOrder.ToLower() == "desc"
                ? notes.OrderByDescending(n => n.Modified)
                : notes.OrderBy(n => n.Modified)
        };

        return sorted;
    }

    public async Task<IEnumerable<Note>> GetRecycledNotesAsync()
    {
        var metadata = await _storageService.LoadMetadataAsync();
        var recycledNotes = new List<Note>();

        foreach (var deletedMetadata in metadata.DeletedNotes)
        {
            var content = await _storageService.ReadNoteFileAsync(deletedMetadata.Id) ?? string.Empty;
            recycledNotes.Add(new Note
            {
                Id = deletedMetadata.Id,
                Filename = deletedMetadata.Filename,
                Title = deletedMetadata.Title,
                Content = content,
                Created = DateTime.MinValue, // 不显示
                Modified = deletedMetadata.Deleted
            });
        }

        return recycledNotes;
    }

    public async Task RestoreNoteAsync(Guid noteId)
    {
        var metadata = await _storageService.LoadMetadataAsync();
        var deletedNote = metadata.DeletedNotes.FirstOrDefault(n => n.Id == noteId);

        if (deletedNote != null)
        {
            // 从回收站恢复文件
            await _storageService.RestoreFromRecycleAsync(noteId);

            // 更新元数据
            metadata.DeletedNotes.Remove(deletedNote);
            metadata.Notes.Add(new NoteMetadata
            {
                Id = noteId,
                Filename = deletedNote.Filename,
                Title = deletedNote.Title,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                WindowX = 100,
                WindowY = 100,
                WindowWidth = 800,
                WindowHeight = 600,
                IsTopmost = false
            });

            await _storageService.SaveMetadataAsync(metadata);

            // 重新加载到缓存
            await LoadNotesFromMetadataAsync();
        }
    }

    public async Task PermanentlyDeleteNoteAsync(Guid noteId)
    {
        var metadata = await _storageService.LoadMetadataAsync();
        var deletedNote = metadata.DeletedNotes.FirstOrDefault(n => n.Id == noteId);

        if (deletedNote != null)
        {
            await _storageService.PermanentlyDeleteFromRecycleAsync(noteId);
            metadata.DeletedNotes.Remove(deletedNote);
            await _storageService.SaveMetadataAsync(metadata);
        }
    }

    public async Task ClearRecycleBinAsync()
    {
        var metadata = await _storageService.LoadMetadataAsync();
        foreach (var deletedNote in metadata.DeletedNotes)
        {
            await _storageService.PermanentlyDeleteFromRecycleAsync(deletedNote.Id);
        }
        metadata.DeletedNotes.Clear();
        await _storageService.SaveMetadataAsync(metadata);
    }

    public async Task ToggleToppmostAsync(Guid noteId)
    {
        var metadata = await _storageService.LoadMetadataAsync();
        var noteMetadata = metadata.Notes.FirstOrDefault(n => n.Id == noteId);

        if (noteMetadata != null)
        {
            noteMetadata.IsTopmost = !noteMetadata.IsTopmost;
            await _storageService.SaveMetadataAsync(metadata);

            var cachedNote = _notesCache.FirstOrDefault(n => n.Id == noteId);
            if (cachedNote != null)
            {
                cachedNote.IsTopmost = noteMetadata.IsTopmost;
            }
        }
    }

    private async Task LoadNotesFromMetadataAsync()
    {
        _notesCache.Clear();
        var metadata = await _storageService.LoadMetadataAsync();

        foreach (var noteMetadata in metadata.Notes)
        {
            _notesCache.Add(new Note
            {
                Id = noteMetadata.Id,
                Filename = noteMetadata.Filename,
                Title = noteMetadata.Title,
                Content = string.Empty, // 延迟加载内容
                Created = noteMetadata.Created,
                Modified = noteMetadata.Modified,
                WindowX = noteMetadata.WindowX,
                WindowY = noteMetadata.WindowY,
                WindowWidth = noteMetadata.WindowWidth,
                WindowHeight = noteMetadata.WindowHeight,
                IsTopmost = noteMetadata.IsTopmost
            });
        }
    }
}