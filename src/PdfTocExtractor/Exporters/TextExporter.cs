using System.Text;
using PdfTocExtractor.Models;

namespace PdfTocExtractor.Exporters;

/// <summary>
/// 纯文本格式导出器
/// </summary>
public class TextExporter : IExporter
{
    public string FormatName => "Text";
    public string FileExtension => "txt";

    public string Export(IEnumerable<TocItem> tocItems, ExportOptions? options = null)
    {
        options ??= new ExportOptions();
        var sb = new StringBuilder();

        // 添加文档标题
        var title = options.CustomTitle ?? "PDF 目录";
        sb.AppendLine(title);
        sb.AppendLine(new string('=', title.Length));
        sb.AppendLine();

        // 导出目录项
        ExportItems(tocItems, sb, options);

        return sb.ToString();
    }

    public async Task ExportToFileAsync(IEnumerable<TocItem> tocItems, string filePath, ExportOptions? options = null)
    {
        var content = Export(tocItems, options);
        options ??= new ExportOptions();
        await File.WriteAllTextAsync(filePath, content, options.Encoding);
    }

    private void ExportItems(IEnumerable<TocItem> items, StringBuilder sb, ExportOptions options, int currentDepth = 0)
    {
        foreach (var item in items)
        {
            // 检查最大深度限制
            if (options.MaxDepth > 0 && currentDepth >= options.MaxDepth)
                continue;

            // 生成缩进
            var indent = new string(' ', item.Level * options.IndentString.Length);

            // 构建项目文本
            var itemText = new StringBuilder();
            itemText.Append($"{indent}- {item.Title}");

            // 添加页码信息
            if (options.IncludePageNumbers && !string.IsNullOrEmpty(item.Page) && item.Page != "无页码" && item.Page != "N/A")
            {
                var pageText = string.Format(options.PageNumberFormat, item.Page);
                itemText.Append($"（{pageText}）");
            }

            sb.AppendLine(itemText.ToString());

            // 递归处理子项目
            if (item.HasChildren)
            {
                ExportItems(item.Children, sb, options, currentDepth + 1);
            }
        }
    }
}
