using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace PdfTocExtractor.Semantic;

/// <summary>
/// PDF文本提取器，提取文本及其样式信息
/// </summary>
public class PdfTextExtractor
{
    /// <summary>
    /// 从PDF文件提取文本片段
    /// </summary>
    public List<TextFragment> ExtractTextFragments(string pdfPath, SemanticAnalysisOptions options)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException($"PDF文件不存在: {pdfPath}");

        var fragments = new List<TextFragment>();

        try
        {
            using var reader = new PdfReader(pdfPath);
            using var pdfDoc = new PdfDocument(reader);

            int totalPages = pdfDoc.GetNumberOfPages();
            
            if (options.DebugMode)
            {
                Console.WriteLine($"开始提取PDF文本: {System.IO.Path.GetFileName(pdfPath)}");
                Console.WriteLine($"总页数: {totalPages}");
                Console.WriteLine($"跳过页面: {string.Join(", ", options.SkipPages)}");
            }

            for (int pageNum = 1; pageNum <= totalPages; pageNum++)
            {
                // 跳过指定页面（通常是目录页）
                if (options.SkipPages.Contains(pageNum))
                {
                    if (options.DebugMode)
                    {
                        Console.WriteLine($"跳过页面 {pageNum}");
                    }
                    continue;
                }

                try
                {
                    var page = pdfDoc.GetPage(pageNum);

                    // 使用简单的文本提取方法
                    string pageText = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page);

                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        // 将页面文本分割成行，每行作为一个文本片段
                        var lines = pageText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                        {
                            var line = lines[lineIndex].Trim();
                            if (!string.IsNullOrWhiteSpace(line) && line.Length > 1)
                            {
                                var fragment = new TextFragment
                                {
                                    Text = line,
                                    FontSize = 12f, // 默认字体大小
                                    FontName = "Unknown",
                                    IsBold = false,
                                    IsItalic = false,
                                    X = 0,
                                    Y = lineIndex * 15, // 估算Y位置
                                    Width = line.Length * 6, // 估算宽度
                                    Height = 12,
                                    PageNumber = pageNum,
                                    IsStandalone = true
                                };

                                fragments.Add(fragment);
                            }
                        }
                    }

                    if (options.DebugMode)
                    {
                        var pageFragments = fragments.Where(f => f.PageNumber == pageNum).ToList();
                        Console.WriteLine($"页面 {pageNum}: 提取到 {pageFragments.Count} 个文本片段");
                    }
                }
                catch (Exception ex)
                {
                    if (options.DebugMode)
                    {
                        Console.WriteLine($"处理页面 {pageNum} 时出错: {ex.Message}");
                    }
                }
            }

            // 合并相邻的文本片段
            var mergedFragments = MergeAdjacentFragments(fragments, options);

            if (options.DebugMode)
            {
                Console.WriteLine($"文本提取完成: {fragments.Count} -> {mergedFragments.Count} 个片段");
            }

            return mergedFragments;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"提取PDF文本时发生错误: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 合并相邻的文本片段
    /// </summary>
    private List<TextFragment> MergeAdjacentFragments(List<TextFragment> fragments, SemanticAnalysisOptions options)
    {
        var mergedFragments = new List<TextFragment>();
        
        // 按页面分组
        var groupedByPage = fragments.GroupBy(f => f.PageNumber);
        
        foreach (var pageGroup in groupedByPage)
        {
            var pageFragments = pageGroup.OrderBy(f => f.Y).ThenBy(f => f.X).ToList();
            
            for (int i = 0; i < pageFragments.Count; i++)
            {
                var current = pageFragments[i];
                var merged = new TextFragment
                {
                    Text = current.Text,
                    FontSize = current.FontSize,
                    FontName = current.FontName,
                    IsBold = current.IsBold,
                    IsItalic = current.IsItalic,
                    X = current.X,
                    Y = current.Y,
                    Width = current.Width,
                    Height = current.Height,
                    PageNumber = current.PageNumber
                };

                // 查找同一行的相邻片段进行合并
                var j = i + 1;
                while (j < pageFragments.Count)
                {
                    var next = pageFragments[j];
                    
                    // 检查是否在同一行
                    if (Math.Abs(next.Y - current.Y) > 3f)
                        break;
                    
                    // 检查是否相邻
                    var expectedX = current.X + current.Width;
                    if (Math.Abs(next.X - expectedX) > 15f)
                        break;
                    
                    // 检查字体是否相同
                    if (Math.Abs(next.FontSize - current.FontSize) > 0.5f)
                        break;
                    
                    // 合并文本
                    merged.Text += next.Text;
                    merged.Width = next.X + next.Width - merged.X;
                    
                    current = next;
                    j++;
                }
                
                // 跳过已合并的片段
                i = j - 1;
                
                // 只保留有意义的文本
                if (!string.IsNullOrWhiteSpace(merged.Text) && merged.Text.Length > 1)
                {
                    mergedFragments.Add(merged);
                }
            }
        }

        // 分析独立成行和垂直间距
        AnalyzeSpacing(mergedFragments);

        return mergedFragments;
    }

    /// <summary>
    /// 分析文本间距
    /// </summary>
    private void AnalyzeSpacing(List<TextFragment> fragments)
    {
        var groupedByPage = fragments.GroupBy(f => f.PageNumber);
        
        foreach (var pageGroup in groupedByPage)
        {
            var pageFragments = pageGroup.OrderBy(f => f.Y).ToList();
            
            for (int i = 0; i < pageFragments.Count; i++)
            {
                var current = pageFragments[i];
                
                // 检查是否独立成行
                var sameLineFragments = pageFragments.Where(f => 
                    Math.Abs(f.Y - current.Y) < 5f && f != current).ToList();
                current.IsStandalone = !sameLineFragments.Any();

                // 计算垂直间距
                if (i > 0)
                {
                    current.VerticalSpaceBefore = Math.Abs(current.Y - pageFragments[i - 1].Y);
                }
                
                if (i < pageFragments.Count - 1)
                {
                    current.VerticalSpaceAfter = Math.Abs(pageFragments[i + 1].Y - current.Y);
                }
            }
        }
    }


}
