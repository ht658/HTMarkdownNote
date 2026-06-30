using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;                      // 修复：Select / Where / OrderBy / ToList
using System.Runtime.CompilerServices;
using System.Threading.Tasks;           // 修复：Task / async
using HTMarkdownNote.Core.Models;
using HTMarkdownNote.Core.Services;

namespace HTMarkdownNote.UI.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly INoteService _noteService;
    private readonly IMarkdownService _markdownService;
    private List<NoteViewModel> _allNotes = new();
    private List<NoteViewModel> _displayNotes = new();
    private string _searchKeyword = string.Empty;
    private string _sortBy = "modified";
    private string _sortOrder = "desc";

    public MainWindowViewModel(INoteService noteService, IMarkdownService markdownService)
    {
        _noteService = noteService ?? throw new ArgumentNullException(nameof(noteService));
        _markdownService = markdownService ?? throw new ArgumentNullException(nameof(markdownService));
    }

    public List<NoteViewModel> DisplayNotes
    {
        get => _displayNotes;
        set
        {
            if (_displayNotes != value)
            {
                _displayNotes = value;
                OnPropertyChanged();
            }
        }
    }

    public string SearchKeyword
    {
        get => _searchKeyword;
        set
        {
            if (_searchKeyword != value)
            {
                _searchKeyword = value;
                OnPropertyChanged();
                _ = UpdateDisplayNotesAsync();
            }
        }
    }

    public string SortBy
    {
        get => _sortBy;
        set
        {
            if (_sortBy != value)
            {
                _sortBy = value;
                OnPropertyChanged();
                _ = UpdateDisplayNotesAsync();
            }
        }
    }

    public string SortOrder
    {
        get => _sortOrder;
        set
        {
            if (_sortOrder != value)
            {
                _sortOrder = value;
                OnPropertyChanged();
                _ = UpdateDisplayNotesAsync();
            }
        }
    }

    public async Task LoadNotesAsync()
    {
        var notes = await _noteService.GetAllNotesAsync();
        _allNotes = notes.Select(n => new NoteViewModel
        {
            Id = n.Id,
            Title = _markdownService.GetFirstLine(n.Content),
            Preview = _markdownService.GetPreview(n.Content, 6),
            Note = n
        }).ToList();
        await UpdateDisplayNotesAsync();
    }

    public async Task CreateNoteAsync()
    {
        await _noteService.CreateNoteAsync();
        await LoadNotesAsync();
    }

    public async Task DeleteNoteAsync(Guid noteId)
    {
        await _noteService.DeleteNoteAsync(noteId);
        await LoadNotesAsync();
    }

    public async Task ToggleToppmostAsync(Guid noteId)
    {
        await _noteService.ToggleToppmostAsync(noteId);
        await LoadNotesAsync();
    }

    private async Task UpdateDisplayNotesAsync()
    {
        IEnumerable<NoteViewModel> result = _allNotes;

        // 搜索过滤
        if (!string.IsNullOrWhiteSpace(_searchKeyword))
        {
            var keyword = _searchKeyword.ToLower();
            result = result.Where(n =>
                n.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                n.Preview.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            );
        }

        // 排序
        result = _sortBy.ToLower() switch
        {
            "created" => _sortOrder.ToLower() == "desc"
                ? result.OrderByDescending(n => n.Note.Created)
                : result.OrderBy(n => n.Note.Created),
            _ => _sortOrder.ToLower() == "desc"
                ? result.OrderByDescending(n => n.Note.Modified)
                : result.OrderBy(n => n.Note.Modified)
        };

        DisplayNotes = result.ToList();
        await Task.CompletedTask;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// 便签视图模型，用于 UI 绑定
/// </summary>
public class NoteViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Preview { get; set; } = string.Empty;
    public Note Note { get; set; } = new();
}
