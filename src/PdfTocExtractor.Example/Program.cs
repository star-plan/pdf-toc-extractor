using PdfTocExtractor;
using PdfTocExtractor.Exporters;

Console.WriteLine("=== PDF TOC Extractor 示例程序 ===");
Console.WriteLine();

// 示例PDF文件路径（您需要替换为实际的PDF文件路径）
var pdfPath = @"C:\path\to\your\document.pdf";

// 检查文件是否存在
if (!File.Exists(pdfPath))
{
    Console.WriteLine("请修改 Program.cs 中的 pdfPath 变量，指向一个实际的PDF文件。");
    Console.WriteLine("当前路径: " + pdfPath);
    return;
}

try
{
    // 创建提取器实例
    var extractor = new PdfTocExtractor.PdfTocExtractor();
    
    Console.WriteLine($"正在从PDF文件提取目录: {Path.GetFileName(pdfPath)}");
    
    // 提取目录
    var tocItems = await extractor.ExtractTocAsync(pdfPath);
    
    Console.WriteLine($"成功提取 {tocItems.Count} 个顶级目录项");
    
    // 显示目录结构
    Console.WriteLine("\n=== 目录结构 ===");
    PrintTocItems(tocItems);
    
    // 导出为不同格式
    var baseFileName = Path.GetFileNameWithoutExtension(pdfPath);
    var outputDir = Path.GetDirectoryName(pdfPath) ?? Environment.CurrentDirectory;
    
    // 配置导出选项
    var exportOptions = new ExportOptions
    {
        CustomTitle = $"{baseFileName} - 目录",
        MaxDepth = 0, // 无限制
        IncludePageNumbers = true,
        IncludeLinks = false
    };
    
    // 导出为Markdown
    var markdownPath = Path.Combine(outputDir, $"{baseFileName}_toc.md");
    await extractor.ExportToFileAsync(tocItems, markdownPath, "markdown", exportOptions);
    Console.WriteLine($"\n✓ Markdown格式已导出到: {markdownPath}");
    
    // 导出为JSON
    var jsonPath = Path.Combine(outputDir, $"{baseFileName}_toc.json");
    await extractor.ExportToFileAsync(tocItems, jsonPath, "json", exportOptions);
    Console.WriteLine($"✓ JSON格式已导出到: {jsonPath}");
    
    // 导出为XML
    var xmlPath = Path.Combine(outputDir, $"{baseFileName}_toc.xml");
    await extractor.ExportToFileAsync(tocItems, xmlPath, "xml", exportOptions);
    Console.WriteLine($"✓ XML格式已导出到: {xmlPath}");
    
    // 导出为纯文本
    var textPath = Path.Combine(outputDir, $"{baseFileName}_toc.txt");
    await extractor.ExportToFileAsync(tocItems, textPath, "text", exportOptions);
    Console.WriteLine($"✓ 纯文本格式已导出到: {textPath}");
    
    // 演示字符串导出
    Console.WriteLine("\n=== Markdown格式预览 ===");
    var markdownContent = extractor.ExportToString(tocItems, "markdown", exportOptions);
    Console.WriteLine(markdownContent.Length > 500 
        ? markdownContent.Substring(0, 500) + "..." 
        : markdownContent);
    
    Console.WriteLine("\n=== 操作完成 ===");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"错误: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"错误: {ex.Message}");

    if (ex.Message.Contains("没有目录（书签）信息"))
    {
        Console.WriteLine();
        Console.WriteLine("这个PDF没有嵌入的书签信息。");
        Console.WriteLine("语义分析功能正在开发中，敬请期待！");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"未知错误: {ex.Message}");
}

static void PrintTocItems(IEnumerable<PdfTocExtractor.Models.TocItem> items, int level = 0)
{
    foreach (var item in items)
    {
        var indent = new string(' ', level * 2);
        Console.WriteLine($"{indent}- {item.Title} (第 {item.Page} 页)");
        
        if (item.HasChildren)
        {
            PrintTocItems(item.Children, level + 1);
        }
    }
}
