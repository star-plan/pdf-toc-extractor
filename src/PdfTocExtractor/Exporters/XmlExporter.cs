using System.Text;
using System.Xml;
using PdfTocExtractor.Models;

namespace PdfTocExtractor.Exporters;

/// <summary>
/// XML格式导出器
/// </summary>
public class XmlExporter : IExporter
{
    public string FormatName => "XML";
    public string FileExtension => "xml";

    public string Export(IEnumerable<TocItem> tocItems, ExportOptions? options = null)
    {
        options ??= new ExportOptions();

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = options.IndentString,
            Encoding = options.Encoding,
            OmitXmlDeclaration = false
        };

        var stringWriter = new StringWriter();
        try
        {
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("PdfTableOfContents");

            // 添加元数据
            xmlWriter.WriteAttributeString("title", options.CustomTitle ?? "PDF 目录");
            xmlWriter.WriteAttributeString("generatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // 导出目录项
            var filteredItems = FilterByDepth(tocItems, options.MaxDepth);
            foreach (var item in filteredItems)
            {
                WriteItem(xmlWriter, item, options);
            }

            xmlWriter.WriteEndElement(); // PdfTableOfContents
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush(); // 确保所有内容都写入StringWriter

            return stringWriter.ToString();
        }
        finally
        {
            stringWriter.Dispose();
        }
    }

    public async Task ExportToFileAsync(IEnumerable<TocItem> tocItems, string filePath, ExportOptions? options = null)
    {
        var content = Export(tocItems, options);
        options ??= new ExportOptions();
        await File.WriteAllTextAsync(filePath, content, options.Encoding);
    }

    private void WriteItem(XmlWriter writer, TocItem item, ExportOptions options)
    {
        writer.WriteStartElement("Item");
        
        // 写入属性
        writer.WriteAttributeString("level", item.Level.ToString());
        if (options.IncludePageNumbers && !string.IsNullOrEmpty(item.Page) && item.Page != "无页码" && item.Page != "N/A")
        {
            writer.WriteAttributeString("page", item.Page);
            if (item.PageNumber > 0)
            {
                writer.WriteAttributeString("pageNumber", item.PageNumber.ToString());
            }
        }
        
        // 写入标题
        writer.WriteElementString("Title", item.Title);
        
        // 写入完整路径
        writer.WriteElementString("FullPath", item.GetFullPath());
        
        // 写入子项目
        if (item.HasChildren)
        {
            writer.WriteStartElement("Children");
            foreach (var child in item.Children)
            {
                WriteItem(writer, child, options);
            }
            writer.WriteEndElement(); // Children
        }
        
        writer.WriteEndElement(); // Item
    }

    private IEnumerable<TocItem> FilterByDepth(IEnumerable<TocItem> items, int maxDepth)
    {
        if (maxDepth <= 0) return items;

        return items.Where(item => item.Level < maxDepth).Select(item => new TocItem
        {
            Title = item.Title,
            Page = item.Page,
            Level = item.Level,
            Parent = item.Parent,
            Children = FilterByDepth(item.Children, maxDepth).ToList()
        });
    }
}
