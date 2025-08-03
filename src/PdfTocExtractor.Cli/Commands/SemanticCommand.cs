using System.CommandLine;
using PdfTocExtractor.Exporters;
using PdfTocExtractor.Semantic;

namespace PdfTocExtractor.Cli.Commands;

/// <summary>
/// 语义分析命令
/// </summary>
public static class SemanticCommand
{
    public static Command Create()
    {
        var inputOption = new Option<FileInfo>(
            aliases: new[] { "--input", "-i" },
            description: "输入PDF文件路径")
        {
            IsRequired = true
        };

        var outputOption = new Option<FileInfo?>(
            aliases: new[] { "--output", "-o" },
            description: "输出文件路径");

        var formatOption = new Option<string?>(
            aliases: new[] { "--format", "-f" },
            description: "输出格式 (markdown, json, xml, text)");

        var maxDepthOption = new Option<int>(
            aliases: new[] { "--max-depth", "-d" },
            description: "最大层级深度",
            getDefaultValue: () => 0);

        var includePagesOption = new Option<bool>(
            aliases: new[] { "--include-pages", "-p" },
            description: "包含页码信息",
            getDefaultValue: () => true);

        var customTitleOption = new Option<string?>(
            aliases: new[] { "--title", "-t" },
            description: "自定义标题");

        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "显示详细输出",
            getDefaultValue: () => false);

        var debugOption = new Option<bool>(
            aliases: new[] { "--debug" },
            description: "启用调试模式",
            getDefaultValue: () => false);

        var modeOption = new Option<string>(
            aliases: new[] { "--mode" },
            description: "分析模式 (default, strict, relaxed)",
            getDefaultValue: () => "default");

        var skipPagesOption = new Option<string>(
            aliases: new[] { "--skip-pages" },
            description: "跳过的页面（如 '1,2,3' 或 '1-3'）",
            getDefaultValue: () => "1,2,3");

        var confidenceOption = new Option<float>(
            aliases: new[] { "--confidence" },
            description: "最小置信度阈值 (0.0-1.0)",
            getDefaultValue: () => 0.3f);

        var fontMultiplierOption = new Option<float>(
            aliases: new[] { "--font-multiplier" },
            description: "字体大小倍数阈值",
            getDefaultValue: () => 1.1f);

        var command = new Command("semantic", "使用语义分析识别PDF目录结构")
        {
            inputOption,
            outputOption,
            formatOption,
            maxDepthOption,
            includePagesOption,
            customTitleOption,
            verboseOption,
            debugOption,
            modeOption,
            skipPagesOption,
            confidenceOption,
            fontMultiplierOption
        };

        command.SetHandler(async (context) =>
        {
            var input = context.ParseResult.GetValueForOption(inputOption)!;
            var output = context.ParseResult.GetValueForOption(outputOption);
            var format = context.ParseResult.GetValueForOption(formatOption);
            var maxDepth = context.ParseResult.GetValueForOption(maxDepthOption);
            var includePages = context.ParseResult.GetValueForOption(includePagesOption);
            var customTitle = context.ParseResult.GetValueForOption(customTitleOption);
            var verbose = context.ParseResult.GetValueForOption(verboseOption);
            var debug = context.ParseResult.GetValueForOption(debugOption);
            var mode = context.ParseResult.GetValueForOption(modeOption)!;
            var skipPages = context.ParseResult.GetValueForOption(skipPagesOption)!;
            var confidence = context.ParseResult.GetValueForOption(confidenceOption);
            var fontMultiplier = context.ParseResult.GetValueForOption(fontMultiplierOption);

            await ExecuteSemanticCommand(input, output, format, maxDepth, includePages, customTitle,
                verbose, debug, mode, skipPages, confidence, fontMultiplier);
        });

        return command;
    }

    private static async Task ExecuteSemanticCommand(
        FileInfo input,
        FileInfo? output,
        string? format,
        int maxDepth,
        bool includePages,
        string? customTitle,
        bool verbose,
        bool debug,
        string mode,
        string skipPages,
        float confidence,
        float fontMultiplier)
    {
        if (!input.Exists)
        {
            throw new FileNotFoundException($"输入文件不存在: {input.FullName}");
        }

        if (verbose)
        {
            Console.WriteLine($"正在使用语义分析处理PDF文件: {input.FullName}");
            Console.WriteLine($"分析模式: {mode}");
            Console.WriteLine($"跳过页面: {skipPages}");
            Console.WriteLine($"置信度阈值: {confidence:F2}");
            Console.WriteLine($"字体倍数: {fontMultiplier:F2}");
        }

        try
        {
            // 创建语义分析选项
            var semanticOptions = CreateSemanticOptions(mode, skipPages, confidence, fontMultiplier, debug);

            var extractor = new PdfTocExtractor();
            var tocItems = await extractor.ExtractTocSemanticAsync(input.FullName, semanticOptions);

            if (verbose)
            {
                Console.WriteLine($"成功识别 {tocItems.Count} 个顶级目录项");
                var totalItems = tocItems.Sum(item => 1 + item.GetAllDescendants().Count());
                Console.WriteLine($"总共 {totalItems} 个目录项");
            }

            if (tocItems.Count == 0)
            {
                Console.WriteLine("未识别到任何目录结构");
                Console.WriteLine("建议:");
                Console.WriteLine("  - 尝试使用 --mode relaxed 降低识别阈值");
                Console.WriteLine("  - 调整 --confidence 参数（如 0.2）");
                Console.WriteLine("  - 使用 --debug 查看详细分析过程");
                Console.WriteLine("  - 检查 --skip-pages 是否正确跳过了目录页");
                return;
            }

            // 创建导出选项
            var exportOptions = new ExportOptions
            {
                MaxDepth = maxDepth,
                IncludePageNumbers = includePages,
                CustomTitle = customTitle ?? "语义分析提取的目录"
            };

            // 导出结果
            if (output != null)
            {
                await extractor.ExportToFileAsync(tocItems, output.FullName, format, exportOptions);
                
                if (verbose)
                {
                    Console.WriteLine($"目录已导出到: {output.FullName}");
                }
            }
            else
            {
                // 输出到控制台
                var outputFormat = format ?? "text";
                var result = extractor.ExportToString(tocItems, outputFormat, exportOptions);
                Console.WriteLine(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"语义分析失败: {ex.Message}");
            if (debug && ex.InnerException != null)
            {
                Console.WriteLine($"详细错误: {ex.InnerException.Message}");
            }
        }
    }

    private static SemanticAnalysisOptions CreateSemanticOptions(
        string mode, 
        string skipPages, 
        float confidence, 
        float fontMultiplier, 
        bool debug)
    {
        // 基础配置
        SemanticAnalysisOptions options = mode.ToLowerInvariant() switch
        {
            "strict" => SemanticAnalysisOptions.Strict,
            "relaxed" => SemanticAnalysisOptions.Relaxed,
            "debug" => SemanticAnalysisOptions.Debug,
            _ => SemanticAnalysisOptions.Default
        };

        // 应用自定义参数
        options.MinConfidenceThreshold = confidence;
        options.FontSizeMultiplier = fontMultiplier;
        options.DebugMode = debug;

        // 解析跳过页面
        options.SkipPages = ParseSkipPages(skipPages);

        return options;
    }

    private static List<int> ParseSkipPages(string skipPages)
    {
        var result = new List<int>();
        
        if (string.IsNullOrWhiteSpace(skipPages))
            return result;

        var parts = skipPages.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            
            if (trimmed.Contains('-'))
            {
                // 范围格式 "1-3"
                var rangeParts = trimmed.Split('-');
                if (rangeParts.Length == 2 && 
                    int.TryParse(rangeParts[0], out var start) && 
                    int.TryParse(rangeParts[1], out var end))
                {
                    for (int i = start; i <= end; i++)
                    {
                        result.Add(i);
                    }
                }
            }
            else if (int.TryParse(trimmed, out var pageNum))
            {
                // 单个页面
                result.Add(pageNum);
            }
        }
        
        return result.Distinct().OrderBy(x => x).ToList();
    }
}
