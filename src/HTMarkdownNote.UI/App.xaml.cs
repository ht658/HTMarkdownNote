using System;
using System.Threading.Tasks;
using System.Windows;
using HTMarkdownNote.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HTMarkdownNote.UI;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
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

            // 手动创建并显示主窗口
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"应用启动失败: {ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        (_serviceProvider as IDisposable)?.Dispose();
    }
}
