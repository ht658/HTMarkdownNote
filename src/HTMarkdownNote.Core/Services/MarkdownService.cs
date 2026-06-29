using HTMarkdownNote.Core.Models;

namespace HTMarkdownNote.Core.Services;

/// <summary>
/// Markdown 服务接口
/// </summary>
public interface IMarkdownService
{
    /// <summary>
    /// 获取第一行作为标题
    /// </summary>
    string GetFirstLine(string markdown);

    /// <summary>
    /// 获取纯文本
    /// </summary>
    string GetPlainText(string markdown);

    /// <summary>
    /// 生成预览文本
    /// </summary>
    string GetPreview(string markdown, int maxLines = 6);
}

/// <summary>
/// Markdown 服务实现
/// </summary>
public class MarkdownService : IMarkdownService
{
    public string GetFirstLine(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return "未命名便签";

        var firstLine = markdown.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        
        // 移除 Markdown 标题符号
        firstLine = System.Text.RegularExpressions.Regex.Replace(firstLine, @"^#+\s*", "");
        
        return string.IsNullOrWhiteSpace(firstLine) ? "未命名便签" : firstLine;
    }

    public string GetPlainText(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var text = markdown;
        
        // 移除标题
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^#+\s*", "");
        
        // 移除加粗、斜体
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*\*(.*?)\*\*", "$1");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"__(.*?)__", "$1");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*(.*?)\*", "$1");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"_(.*?)_", "$1");
        
        // 移除链接
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\[(.*?)\]\((.*?)\)", "$1");
        
        return text.Trim();
    }

    public string GetPreview(string markdown, int maxLines = 6)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var lines = markdown.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
        var previewLines = new List<string>();
        int lineCount = 0;

        foreach (var line in lines)
        {
            if (lineCount >= maxLines)
                break;

            var trimmed = line.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                previewLines.Add(trimmed);
                lineCount++;
            }
        }

        return string.Join(" ", previewLines);
    }
}