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

        // 过滤深度并导出目录项
        var filteredItems = FilterByDepth(tocItems, options.MaxDepth);
        ExportItems(filteredItems, sb, options);

        return sb.ToString();
    }

    public async Task ExportToFileAsync(IEnumerable<TocItem> tocItems, string filePath, ExportOptions? options = null)
    {
        var content = Export(tocItems, options);
        options ??= new ExportOptions();
        await File.WriteAllTextAsync(filePath, content, options.Encoding);
    }

    private void ExportItems(IEnumerable<TocItem> items, StringBuilder sb, ExportOptions options)
    {
        foreach (var item in items)
        {
            // 生成缩进
            var indent = string.Concat(Enumerable.Repeat(options.IndentString, item.Level));

            // 构建项目文本
            var itemText = new StringBuilder();
            itemText.Append($"{indent}- {item.Title}");

            // 添加页码信息
            if (options.IncludePageNumbers && !string.IsNullOrEmpty(item.Page) && item.Page != "无页码" && item.Page != "N/A")
            {
                var pageText = string.Format(options.PageNumberFormat, item.Page);
                // 根据页码格式决定括号类型：默认中文格式使用中文括号，自定义格式使用英文括号
                var isDefaultFormat = options.PageNumberFormat == "第 {0} 页";
                var brackets = isDefaultFormat ? ("（", "）") : ("(", ")");
                // 中文格式不需要空格，英文格式需要空格
                var spacing = isDefaultFormat ? "" : " ";
                itemText.Append($"{spacing}{brackets.Item1}{pageText}{brackets.Item2}");
            }

            sb.AppendLine(itemText.ToString());

            // 递归处理子项目
            if (item.HasChildren)
            {
                ExportItems(item.Children, sb, options);
            }
        }
    }

    private IEnumerable<TocItem> FilterByDepth(IEnumerable<TocItem> items, int maxDepth)
    {
        if (maxDepth <= 0) return items;

        return items.Where(item => item.Level <= maxDepth).Select(item => new TocItem
        {
            Title = item.Title,
            Page = item.Page,
            Level = item.Level,
            Parent = item.Parent,
            Children = FilterByDepth(item.Children, maxDepth).ToList()
        });
    }
}
