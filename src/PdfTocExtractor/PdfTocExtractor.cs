using iText.Kernel.Pdf;
using PdfTocExtractor.Exporters;
using PdfTocExtractor.Models;

namespace PdfTocExtractor;

/// <summary>
/// PDF目录提取器
/// </summary>
public class PdfTocExtractor
{
    private readonly Dictionary<string, IExporter> _exporters;

    public PdfTocExtractor()
    {
        _exporters = new Dictionary<string, IExporter>(StringComparer.OrdinalIgnoreCase)
        {
            ["markdown"] = new MarkdownExporter(),
            ["md"] = new MarkdownExporter(),
            ["json"] = new JsonExporter(),
            ["xml"] = new XmlExporter(),
            ["text"] = new TextExporter(),
            ["txt"] = new TextExporter()
        };
    }

    /// <summary>
    /// 从PDF文件提取目录
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <returns>目录项目列表</returns>
    /// <exception cref="FileNotFoundException">PDF文件不存在</exception>
    /// <exception cref="InvalidOperationException">PDF文件无法读取或没有目录信息</exception>
    public List<TocItem> ExtractToc(string pdfPath)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException($"PDF文件不存在: {pdfPath}");

        try
        {
            using var reader = new PdfReader(pdfPath);
            using var pdfDoc = new PdfDocument(reader);

            var outlines = pdfDoc.GetOutlines(false);
            if (outlines == null)
                throw new InvalidOperationException("此PDF文件没有目录（书签）信息");

            var bookmarks = outlines.GetAllChildren();
            if (bookmarks == null || bookmarks.Count == 0)
                throw new InvalidOperationException("此PDF文件没有目录（书签）信息");

            return ConvertBookmarksToTocItems(bookmarks, pdfDoc);
        }
        catch (Exception ex) when (!(ex is FileNotFoundException || ex is InvalidOperationException))
        {
            throw new InvalidOperationException($"读取PDF文件时发生错误: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步从PDF文件提取目录
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <returns>目录项目列表</returns>
    public Task<List<TocItem>> ExtractTocAsync(string pdfPath)
    {
        return Task.Run(() => ExtractToc(pdfPath));
    }

    /// <summary>
    /// 导出目录到指定格式的字符串
    /// </summary>
    /// <param name="tocItems">目录项目列表</param>
    /// <param name="format">导出格式</param>
    /// <param name="options">导出选项</param>
    /// <returns>导出的字符串内容</returns>
    public string ExportToString(IEnumerable<TocItem> tocItems, string format, ExportOptions? options = null)
    {
        var exporter = GetExporter(format);
        return exporter.Export(tocItems, options);
    }

    /// <summary>
    /// 导出目录到文件
    /// </summary>
    /// <param name="tocItems">目录项目列表</param>
    /// <param name="outputPath">输出文件路径</param>
    /// <param name="format">导出格式（如果为空则根据文件扩展名推断）</param>
    /// <param name="options">导出选项</param>
    public async Task ExportToFileAsync(IEnumerable<TocItem> tocItems, string outputPath, string? format = null, ExportOptions? options = null)
    {
        format ??= Path.GetExtension(outputPath).TrimStart('.');
        var exporter = GetExporter(format);
        await exporter.ExportToFileAsync(tocItems, outputPath, options);
    }

    /// <summary>
    /// 从PDF文件提取目录并直接导出到文件
    /// </summary>
    /// <param name="pdfPath">PDF文件路径</param>
    /// <param name="outputPath">输出文件路径</param>
    /// <param name="format">导出格式（如果为空则根据文件扩展名推断）</param>
    /// <param name="options">导出选项</param>
    public async Task ExtractAndExportAsync(string pdfPath, string outputPath, string? format = null, ExportOptions? options = null)
    {
        var tocItems = await ExtractTocAsync(pdfPath);
        await ExportToFileAsync(tocItems, outputPath, format, options);
    }

    /// <summary>
    /// 获取支持的导出格式列表
    /// </summary>
    public IEnumerable<string> GetSupportedFormats()
    {
        return _exporters.Keys.Distinct(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 注册自定义导出器
    /// </summary>
    /// <param name="format">格式名称</param>
    /// <param name="exporter">导出器实例</param>
    public void RegisterExporter(string format, IExporter exporter)
    {
        if (string.IsNullOrEmpty(format))
            throw new ArgumentException("Format cannot be null or empty.", nameof(format));

        if (exporter == null)
            throw new ArgumentNullException(nameof(exporter));

        _exporters[format.ToLowerInvariant()] = exporter;
    }

    private IExporter GetExporter(string format)
    {
        if (string.IsNullOrEmpty(format))
            throw new ArgumentException("导出格式不能为空", nameof(format));

        if (!_exporters.TryGetValue(format, out var exporter))
            throw new NotSupportedException($"不支持的导出格式: {format}。支持的格式: {string.Join(", ", GetSupportedFormats())}");

        return exporter;
    }

    /// <summary>
    /// 将iText的书签转换为TocItem对象
    /// </summary>
    private List<TocItem> ConvertBookmarksToTocItems(IList<PdfOutline> bookmarks, PdfDocument pdfDoc, TocItem? parent = null, int level = 0)
    {
        var tocItems = new List<TocItem>();

        foreach (var bookmark in bookmarks)
        {
            var tocItem = new TocItem
            {
                Title = GetBookmarkTitle(bookmark),
                Page = GetBookmarkPage(bookmark, pdfDoc),
                Level = level,
                Parent = parent
            };

            // 处理子书签
            var children = bookmark.GetAllChildren();
            if (children != null && children.Count > 0)
            {
                tocItem.Children = ConvertBookmarksToTocItems(children, pdfDoc, tocItem, level + 1);
            }

            tocItems.Add(tocItem);
        }

        return tocItems;
    }

    /// <summary>
    /// 获取书签标题
    /// </summary>
    private static string GetBookmarkTitle(PdfOutline bookmark)
    {
        var title = bookmark.GetTitle();
        return string.IsNullOrEmpty(title) ? "无标题" : title;
    }

    /// <summary>
    /// 获取书签页码
    /// </summary>
    private static string GetBookmarkPage(PdfOutline bookmark, PdfDocument pdfDoc)
    {
        try
        {
            var destination = bookmark.GetDestination();
            if (destination != null && destination.GetPdfObject() != null)
            {
                // 尝试从目标对象中提取页码信息
                var destObj = destination.GetPdfObject();
                if (destObj.IsArray())
                {
                    var array = (PdfArray)destObj;
                    if (array.Size() > 0)
                    {
                        var pageRef = array.Get(0);
                        if (pageRef != null && pageRef.IsIndirectReference())
                        {
                            // 通过页面引用获取页码
                            var pageDict = pageRef.GetIndirectReference().GetRefersTo();
                            if (pageDict != null)
                            {
                                // 遍历所有页面找到匹配的页码
                                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                                {
                                    var page = pdfDoc.GetPage(i);
                                    if (page.GetPdfObject().Equals(pageDict))
                                    {
                                        return i.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // 如果获取页码失败，返回默认值
        }

        return "无页码";
    }


}
