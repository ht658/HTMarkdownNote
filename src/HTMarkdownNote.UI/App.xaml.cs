using System;
using System.Threading.Tasks;
using HTMarkdownNote.Core.Services;
using HTMarkdownNote.Core.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace HTMarkdownNote.UI;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 配置依赖注入
        var services = new ServiceCollection();
        
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<INoteService, NoteService>();
        services.AddSingleton<IMarkdownService, MarkdownService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        // 初始化存储服务
        var storageService = _serviceProvider.GetRequiredService<IStorageService>();
        await storageService.InitializeAsync();

        // 初始化 NoteService
        var noteService = _serviceProvider.GetRequiredService<INoteService>();
        if (noteService is NoteService noteServiceImpl)
        {
            await noteServiceImpl.InitializeAsync();
        }

        // 打开主窗口
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        _serviceProvider?.Dispose();
    }
}