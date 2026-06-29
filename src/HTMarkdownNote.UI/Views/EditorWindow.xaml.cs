using System.Windows;
using HTMarkdownNote.Core.Models;
using HTMarkdownNote.Core.Services;

namespace HTMarkdownNote.UI;

public partial class EditorWindow : Window
{
    private readonly INoteService _noteService;
    private readonly IMarkdownService _markdownService;
    private Note? _currentNote;
    private System.Windows.Threading.DispatcherTimer? _autoSaveTimer;

    public EditorWindow(INoteService noteService, IMarkdownService markdownService)
    {
        InitializeComponent();
        _noteService = noteService ?? throw new ArgumentNullException(nameof(noteService));
        _markdownService = markdownService ?? throw new ArgumentNullException(nameof(markdownService));

        // 初始化自动保存计时器（300ms 延迟）
        _autoSaveTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _autoSaveTimer.Tick += AutoSaveTimer_Tick;
    }

    public void SetNote(Note note)
    {
        _currentNote = note ?? throw new ArgumentNullException(nameof(note));
        Title = $"编辑: {_markdownService.GetFirstLine(note.Content)}";
        Editor.Text = note.Content;

        // 恢复窗口位置和大小
        if (note.WindowX > 0 && note.WindowY > 0)
        {
            Left = note.WindowX;
            Top = note.WindowY;
        }

        Width = note.WindowWidth;
        Height = note.WindowHeight;
        Topmost = note.IsTopmost;
    }

    private void Editor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // 重置自动保存计时器
        _autoSaveTimer?.Stop();
        _autoSaveTimer?.Start();
    }

    private async void AutoSaveTimer_Tick(object? sender, EventArgs e)
    {
        _autoSaveTimer?.Stop();

        if (_currentNote != null)
        {
            _currentNote.Content = Editor.Text;
            _currentNote.Title = _markdownService.GetFirstLine(_currentNote.Content);
            _currentNote.Modified = DateTime.UtcNow;
            _currentNote.WindowX = (int)Left;
            _currentNote.WindowY = (int)Top;
            _currentNote.WindowWidth = (int)Width;
            _currentNote.WindowHeight = (int)Height;

            await _noteService.UpdateNoteAsync(_currentNote);
        }
    }

    private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // 确保最后的内容被保存
        if (_autoSaveTimer?.IsEnabled == true)
        {
            _autoSaveTimer.Stop();
            // 同步保存
            if (_currentNote != null)
            {
                _currentNote.Content = Editor.Text;
                _currentNote.Title = _markdownService.GetFirstLine(_currentNote.Content);
                _currentNote.Modified = DateTime.UtcNow;
                _currentNote.WindowX = (int)Left;
                _currentNote.WindowY = (int)Top;
                _currentNote.WindowWidth = (int)Width;
                _currentNote.WindowHeight = (int)Height;
                await _noteService.UpdateNoteAsync(_currentNote);
            }
        }
    }
}