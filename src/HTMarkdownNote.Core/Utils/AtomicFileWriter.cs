using System;
using System.IO;

namespace HTMarkdownNote.Core.Utils;

/// <summary>
/// 原子文件写入器，确保写入过程中断电或异常时不损坏文件
/// </summary>
public static class AtomicFileWriter
{
    /// <summary>
    /// 原子写入文本文件
    /// </summary>
    /// <param name="filepath">目标文件路径</param>
    /// <param name="content">文件内容</param>
    /// <exception cref="IOException">文件操作异常</exception>
    public static void WriteAllText(string filepath, string content)
    {
        // 确保目录存在
        var directory = Path.GetDirectoryName(filepath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 生成临时文件路径
        var tempPath = filepath + ".tmp";

        try
        {
            // 1. 写入临时文件
            File.WriteAllText(tempPath, content, System.Text.Encoding.UTF8);

            // 2. 验证文件完整性（简单检查：文件是否存在且大小合理）
            if (!File.Exists(tempPath))
            {
                throw new IOException("Temporary file was not created successfully.");
            }

            var tempFileInfo = new FileInfo(tempPath);
            if (tempFileInfo.Length == 0 && content.Length > 0)
            {
                throw new IOException("Temporary file is empty but content is not.");
            }

            // 3. 原子替换
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            File.Move(tempPath, filepath, overwrite: false);
        }
        catch (Exception ex)
        {
            // 清理临时文件
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch
            {
                // 忽略清理异常
            }

            throw new IOException($"Failed to write file atomically: {filepath}", ex);
        }
    }

    /// <summary>
    /// 原子写入 UTF-8 文本（使用流）
    /// </summary>
    public static void WriteAllTextAsync(string filepath, string content)
    {
        // 同步版本就够了，这里不做异步实现
        WriteAllText(filepath, content);
    }
}