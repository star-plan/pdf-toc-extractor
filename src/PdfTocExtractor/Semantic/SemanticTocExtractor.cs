using PdfTocExtractor.Models;

namespace PdfTocExtractor.Semantic;

/// <summary>
/// 基于语义分析的目录提取器
/// </summary>
public class SemanticTocExtractor
{
    private readonly SemanticAnalysisOptions _options;
    private readonly PdfTextExtractor _textExtractor;
    private readonly SemanticHeadingAnalyzer _headingAnalyzer;

    public SemanticTocExtractor(SemanticAnalysisOptions? options = null)
    {
        _options = options ?? SemanticAnalysisOptions.Default;
        _textExtractor = new PdfTextExtractor();
        _headingAnalyzer = new SemanticHeadingAnalyzer(_options);
    }

    /// <summary>
    /// 从PDF文件提取目录
    /// </summary>
    public List<TocItem> ExtractToc(string pdfPath)
    {
        if (!File.Exists(pdfPath))
            throw new FileNotFoundException($"PDF文件不存在: {pdfPath}");

        try
        {
            if (_options.DebugMode)
            {
                Console.WriteLine("=== 开始语义分析目录提取 ===");
                Console.WriteLine($"文件: {Path.GetFileName(pdfPath)}");
                Console.WriteLine($"配置: {GetOptionsDescription()}");
                Console.WriteLine();
            }

            // 1. 提取文本片段
            var fragments = _textExtractor.ExtractTextFragments(pdfPath, _options);

            if (!fragments.Any())
            {
                if (_options.DebugMode)
                {
                    Console.WriteLine("未提取到任何文本片段");
                }
                return new List<TocItem>();
            }

            // 2. 语义分析识别标题
            var headings = _headingAnalyzer.AnalyzeHeadings(fragments);

            if (!headings.Any())
            {
                if (_options.DebugMode)
                {
                    Console.WriteLine("未识别到任何标题");
                }
                return new List<TocItem>();
            }

            // 3. 转换为TocItem
            var tocItems = ConvertToTocItems(headings);

            if (_options.DebugMode)
            {
                Console.WriteLine($"\n=== 最终结果 ===");
                Console.WriteLine($"识别到 {tocItems.Count} 个顶级目录项");
                var totalItems = tocItems.Sum(item => 1 + item.GetAllDescendants().Count());
                Console.WriteLine($"总共 {totalItems} 个目录项");
                
                Console.WriteLine("\n目录结构预览:");
                PrintTocPreview(tocItems, 0, 5);
            }

            return tocItems;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"语义分析提取目录时发生错误: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步提取目录
    /// </summary>
    public Task<List<TocItem>> ExtractTocAsync(string pdfPath)
    {
        return Task.Run(() => ExtractToc(pdfPath));
    }

    /// <summary>
    /// 将标题片段转换为TocItem
    /// </summary>
    private List<TocItem> ConvertToTocItems(List<TextFragment> headings)
    {
        var tocItems = new List<TocItem>();
        var levelStack = new Stack<TocItem>();

        foreach (var heading in headings)
        {
            var level = heading.SemanticResult?.EstimatedLevel ?? 1;
            
            var tocItem = new TocItem
            {
                Title = CleanHeadingText(heading.Text),
                Page = heading.PageNumber.ToString(),
                Level = level - 1, // 转换为0基索引
                Children = new List<TocItem>()
            };

            // 建立层级关系
            while (levelStack.Count > 0 && levelStack.Peek().Level >= tocItem.Level)
            {
                levelStack.Pop();
            }

            if (levelStack.Count == 0)
            {
                tocItems.Add(tocItem);
            }
            else
            {
                var parent = levelStack.Peek();
                tocItem.Parent = parent;
                parent.Children.Add(tocItem);
            }

            levelStack.Push(tocItem);
        }

        return tocItems;
    }

    /// <summary>
    /// 清理标题文本
    /// </summary>
    private string CleanHeadingText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // 移除多余的空白字符
        text = text.Trim();
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

        return text;
    }

    /// <summary>
    /// 获取配置描述
    /// </summary>
    private string GetOptionsDescription()
    {
        return $"置信度阈值: {_options.MinConfidenceThreshold:F2}, " +
               $"字体倍数: {_options.FontSizeMultiplier:F2}, " +
               $"跳过页面: [{string.Join(",", _options.SkipPages)}]";
    }

    /// <summary>
    /// 打印目录预览
    /// </summary>
    private void PrintTocPreview(List<TocItem> items, int currentLevel, int maxItems)
    {
        var count = 0;
        foreach (var item in items)
        {
            if (count >= maxItems)
            {
                Console.WriteLine($"{new string(' ', currentLevel * 2)}... 还有 {items.Count - count} 个项目");
                break;
            }

            var indent = new string(' ', currentLevel * 2);
            Console.WriteLine($"{indent}- [{item.Level}] {item.Title} (第 {item.Page} 页)");
            
            if (item.Children.Any() && currentLevel < 2)
            {
                PrintTocPreview(item.Children, currentLevel + 1, 3);
            }
            
            count++;
        }
    }

    /// <summary>
    /// 获取分析统计信息
    /// </summary>
    public SemanticAnalysisStatistics GetAnalysisStatistics(string pdfPath)
    {
        try
        {
            var fragments = _textExtractor.ExtractTextFragments(pdfPath, _options);
            var headings = _headingAnalyzer.AnalyzeHeadings(fragments);

            var stats = new SemanticAnalysisStatistics
            {
                TotalTextFragments = fragments.Count,
                IdentifiedHeadings = headings.Count,
                AverageConfidence = headings.Any() ? 
                    headings.Average(h => h.SemanticResult?.HeadingConfidence ?? 0) : 0,
                HeadingsByLevel = headings
                    .GroupBy(h => h.SemanticResult?.EstimatedLevel ?? 0)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageFontSize = fragments.Any() ? fragments.Average(f => f.FontSize) : 0,
                BoldTextCount = fragments.Count(f => f.IsBold)
            };

            return stats;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取分析统计信息时发生错误: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// 语义分析统计信息
/// </summary>
public class SemanticAnalysisStatistics
{
    public int TotalTextFragments { get; set; }
    public int IdentifiedHeadings { get; set; }
    public float AverageConfidence { get; set; }
    public Dictionary<int, int> HeadingsByLevel { get; set; } = new();
    public float AverageFontSize { get; set; }
    public int BoldTextCount { get; set; }
}
