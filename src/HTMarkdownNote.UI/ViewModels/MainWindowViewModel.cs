using System.ComponentModel;
using System.Runtime.CompilerServices;
using HTMarkdownNote.Core.Models;
using HTMarkdownNote.Core.Services;

namespace HTMarkdownNote.UI.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly INoteService _noteService;
    private readonly IMarkdownService _markdownService;
    private List<Note> _allNotes = new();
    private List<Note> _displayNotes = new();
    private string _searchKeyword = string.Empty;
    private string _sortBy = "modified";
    private string _sortOrder = "desc";

    public MainWindowViewModel(INoteService noteService, IMarkdownService markdownService)
    {
        _noteService = noteService ?? throw new ArgumentNullException(nameof(noteService));
        _markdownService = markdownService ?? throw new ArgumentNullException(nameof(markdownService));
    }

    public List<Note> DisplayNotes
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
        _allNotes = (await _noteService.GetAllNotesAsync()).ToList();
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

    public string GetNotePreview(Note note)
    {
        return _markdownService.GetPreview(note.Content, 6);
    }

    public string GetNoteTitle(Note note)
    {
        return _markdownService.GetFirstLine(note.Content);
    }

    private async Task UpdateDisplayNotesAsync()
    {
        IEnumerable<Note> result = _allNotes;

        // 搜索过滤
        if (!string.IsNullOrWhiteSpace(_searchKeyword))
        {
            result = await _noteService.SearchNotesAsync(_searchKeyword);
        }

        // 排序
        result = await _noteService.GetSortedNotesAsync(_sortBy, _sortOrder);

        DisplayNotes = result.ToList();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}