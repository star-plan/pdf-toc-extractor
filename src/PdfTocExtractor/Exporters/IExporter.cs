using PdfTocExtractor.Models;

namespace PdfTocExtractor.Exporters;

/// <summary>
/// 目录导出器接口
/// </summary>
public interface IExporter
{
    /// <summary>
    /// 导出格式名称
    /// </summary>
    string FormatName { get; }

    /// <summary>
    /// 文件扩展名（不包含点）
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// 导出目录项目到字符串
    /// </summary>
    /// <param name="tocItems">目录项目列表</param>
    /// <param name="options">导出选项</param>
    /// <returns>导出的字符串内容</returns>
    string Export(IEnumerable<TocItem> tocItems, ExportOptions? options = null);

    /// <summary>
    /// 异步导出目录项目到文件
    /// </summary>
    /// <param name="tocItems">目录项目列表</param>
    /// <param name="filePath">输出文件路径</param>
    /// <param name="options">导出选项</param>
    Task ExportToFileAsync(IEnumerable<TocItem> tocItems, string filePath, ExportOptions? options = null);
}

/// <summary>
/// 导出选项
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// 缩进字符串（默认为两个空格）
    /// </summary>
    public string IndentString { get; set; } = "  ";

    /// <summary>
    /// 是否包含页码
    /// </summary>
    public bool IncludePageNumbers { get; set; } = true;

    /// <summary>
    /// 是否包含链接（如果格式支持）
    /// </summary>
    public bool IncludeLinks { get; set; } = false;

    /// <summary>
    /// 最大层级深度（0表示无限制）
    /// </summary>
    public int MaxDepth { get; set; } = 0;

    /// <summary>
    /// 页码格式化字符串
    /// </summary>
    public string PageNumberFormat { get; set; } = "第 {0} 页";

    /// <summary>
    /// 自定义标题（用于某些格式的文档标题）
    /// </summary>
    public string? CustomTitle { get; set; }

    /// <summary>
    /// 编码格式（默认UTF-8）
    /// </summary>
    public System.Text.Encoding Encoding { get; set; } = System.Text.Encoding.UTF8;
}
