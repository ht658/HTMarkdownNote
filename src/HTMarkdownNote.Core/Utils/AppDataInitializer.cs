using HTMarkdownNote.Core.Constants;

namespace HTMarkdownNote.Core.Utils;

/// <summary>
/// 应用数据目录初始化器
/// </summary>
public static class AppDataInitializer
{
    /// <summary>
    /// 初始化应用数据目录结构
    /// </summary>
    public static void Initialize()
    {
        try
        {
            // 创建主目录
            if (!Directory.Exists(AppConstants.AppDataPath))
            {
                Directory.CreateDirectory(AppConstants.AppDataPath);
            }

            // 创建子目录
            CreateDirectoryIfNotExists(AppConstants.NotesPath);
            CreateDirectoryIfNotExists(AppConstants.BackupsPath);
            CreateDirectoryIfNotExists(AppConstants.RecyclePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize app data directory: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 如果目录不存在则创建
    /// </summary>
    private static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}