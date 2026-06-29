using System.Windows;
using HTMarkdownNote.Core.Services;
using HTMarkdownNote.UI.ViewModels;

namespace HTMarkdownNote.UI;

public partial class MainWindow : Window
{
    private readonly INoteService _noteService;
    private readonly IMarkdownService _markdownService;
    private MainWindowViewModel? _viewModel;

    public MainWindow(INoteService noteService, IMarkdownService markdownService)
    {
        InitializeComponent();
        _noteService = noteService ?? throw new ArgumentNullException(nameof(noteService));
        _markdownService = markdownService ?? throw new ArgumentNullException(nameof(markdownService));
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // 初始化 ViewModel
        _viewModel = new MainWindowViewModel(_noteService, _markdownService);
        DataContext = _viewModel;

        // 加载便签
        await _viewModel.LoadNotesAsync();
    }

    private async void NewNoteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            await _viewModel.CreateNoteAsync();
        }
    }

    private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.SearchKeyword = SearchBox.Text;
        }
    }

    private async void NoteListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (NoteListBox.SelectedItem is Note note)
        {
            // 在新窗口打开编辑
            var editorWindow = new EditorWindow(_noteService, _markdownService) { Owner = this };
            editorWindow.SetNote(note);
            editorWindow.Show();
        }
    }

    private async void MoreButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: 实现右键菜单（删除、置顶）
    }
}

public partial class MainWindow : Window
{
    // 为了使用 Note 类型，需要添加 using
}
