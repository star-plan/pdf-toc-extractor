using System.Text.Json;
using System.Text.Json.Serialization;
using PdfTocExtractor.Models;

namespace PdfTocExtractor.Exporters;

/// <summary>
/// JSON格式导出器
/// </summary>
public class JsonExporter : IExporter
{
    public string FormatName => "JSON";
    public string FileExtension => "json";

    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string Export(IEnumerable<TocItem> tocItems, ExportOptions? options = null)
    {
        options ??= new ExportOptions();
        
        var exportData = new
        {
            Title = options.CustomTitle ?? "PDF 目录",
            GeneratedAt = DateTime.Now,
            Items = FilterByDepth(tocItems, options.MaxDepth).Select(item => ConvertToJsonObject(item, options))
        };

        return JsonSerializer.Serialize(exportData, DefaultJsonOptions);
    }

    public async Task ExportToFileAsync(IEnumerable<TocItem> tocItems, string filePath, ExportOptions? options = null)
    {
        var content = Export(tocItems, options);
        options ??= new ExportOptions();
        await File.WriteAllTextAsync(filePath, content, options.Encoding);
    }

    private object ConvertToJsonObject(TocItem item, ExportOptions options)
    {
        var result = new Dictionary<string, object>
        {
            ["title"] = item.Title,
            ["level"] = item.Level
        };

        if (options.IncludePageNumbers && !string.IsNullOrEmpty(item.Page) && item.Page != "无页码" && item.Page != "N/A")
        {
            result["page"] = item.Page;
            if (item.PageNumber > 0)
            {
                result["pageNumber"] = item.PageNumber;
            }
        }

        if (item.HasChildren)
        {
            result["children"] = item.Children.Select(child => ConvertToJsonObject(child, options)).ToArray();
        }

        // 添加完整路径信息
        result["fullPath"] = item.GetFullPath();

        return result;
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
