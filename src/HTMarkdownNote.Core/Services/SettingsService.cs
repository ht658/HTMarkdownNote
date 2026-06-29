using HTMarkdownNote.Core.Models;

namespace HTMarkdownNote.Core.Services;

/// <summary>
/// 设置服务接口
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// 加载设置
    /// </summary>
    Task<AppSettings> LoadSettingsAsync();

    /// <summary>
    /// 保存设置
    /// </summary>
    Task SaveSettingsAsync(AppSettings settings);
}

/// <summary>
/// 设置服务实现
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly IStorageService _storageService;

    public SettingsService(IStorageService storageService)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            var metadata = await _storageService.LoadMetadataAsync();
            return metadata.Settings ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        var metadata = await _storageService.LoadMetadataAsync();
        metadata.Settings = settings;
        await _storageService.SaveMetadataAsync(metadata);
    }
}