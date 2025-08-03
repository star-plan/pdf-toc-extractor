using System.CommandLine;
using PdfTocExtractor.Exporters;
using PdfTocExtractor.Models;
using PdfTocExtractor.Semantic;

namespace PdfTocExtractor.Cli.Commands;

/// <summary>
/// 智能提取命令
/// </summary>
public static class SmartCommand
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

        var includeLinksOption = new Option<bool>(
            aliases: new[] { "--include-links", "-l" },
            description: "包含链接信息",
            getDefaultValue: () => false);

        var customTitleOption = new Option<string?>(
            aliases: new[] { "--title", "-t" },
            description: "自定义标题");

        var indentOption = new Option<string>(
            aliases: new[] { "--indent" },
            description: "缩进字符串",
            getDefaultValue: () => "  ");

        var pageFormatOption = new Option<string>(
            aliases: new[] { "--page-format" },
            description: "页码格式字符串",
            getDefaultValue: () => "{0}");

        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "显示详细输出",
            getDefaultValue: () => false);



        var command = new Command("smart", "智能提取目录：先尝试提取书签，失败则分析结构")
        {
            inputOption,
            outputOption,
            formatOption,
            maxDepthOption,
            includePagesOption,
            includeLinksOption,
            customTitleOption,
            indentOption,
            pageFormatOption,
            verboseOption,

        };

        command.SetHandler(async (context) =>
        {
            var input = context.ParseResult.GetValueForOption(inputOption)!;
            var output = context.ParseResult.GetValueForOption(outputOption);
            var format = context.ParseResult.GetValueForOption(formatOption);
            var maxDepth = context.ParseResult.GetValueForOption(maxDepthOption);
            var includePages = context.ParseResult.GetValueForOption(includePagesOption);
            var includeLinks = context.ParseResult.GetValueForOption(includeLinksOption);
            var customTitle = context.ParseResult.GetValueForOption(customTitleOption);
            var indent = context.ParseResult.GetValueForOption(indentOption);
            var pageFormat = context.ParseResult.GetValueForOption(pageFormatOption);
            var verbose = context.ParseResult.GetValueForOption(verboseOption);
            await ExecuteSmartCommand(input, output, format, maxDepth, includePages, includeLinks,
                customTitle, indent, pageFormat, verbose);
        });

        return command;
    }

    private static async Task ExecuteSmartCommand(
        FileInfo input,
        FileInfo? output,
        string? format,
        int maxDepth,
        bool includePages,
        bool includeLinks,
        string? customTitle,
        string indent,
        string pageFormat,
        bool verbose)
    {
        if (!input.Exists)
        {
            throw new FileNotFoundException($"输入文件不存在: {input.FullName}");
        }

        if (verbose)
        {
            Console.WriteLine($"正在智能处理PDF文件: {input.FullName}");
        }

        var extractor = new PdfTocExtractor();
        List<TocItem> tocItems;

        try
        {
            // 尝试提取书签
            if (verbose)
            {
                Console.WriteLine("尝试提取PDF书签...");
            }

            tocItems = await extractor.ExtractTocAsync(input.FullName);

            if (verbose)
            {
                Console.WriteLine("成功提取PDF书签");
                Console.WriteLine($"成功提取 {tocItems.Count} 个顶级目录项");
                var totalItems = tocItems.Sum(item => 1 + item.GetAllDescendants().Count());
                Console.WriteLine($"总共 {totalItems} 个目录项");
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("没有目录（书签）信息"))
        {
            if (verbose)
            {
                Console.WriteLine("PDF文件没有书签信息，切换到语义分析模式...");
            }

            try
            {
                // 使用语义分析
                var semanticOptions = SemanticAnalysisOptions.Default;
                tocItems = await extractor.ExtractTocSemanticAsync(input.FullName, semanticOptions);

                if (verbose)
                {
                    Console.WriteLine("语义分析完成");
                    Console.WriteLine($"成功识别 {tocItems.Count} 个顶级目录项");
                    var totalItems = tocItems.Sum(item => 1 + item.GetAllDescendants().Count());
                    Console.WriteLine($"总共 {totalItems} 个目录项");
                }
            }
            catch (Exception semanticEx)
            {
                Console.WriteLine("错误: PDF文件没有书签信息，语义分析也失败了");
                Console.WriteLine($"语义分析错误: {semanticEx.Message}");
                Console.WriteLine("建议:");
                Console.WriteLine("  - 使用 'semantic' 命令进行更精细的语义分析");
                Console.WriteLine("  - 尝试调整语义分析参数");
                return;
            }
        }

        if (tocItems.Count == 0)
        {
            Console.WriteLine("警告: 未能提取到任何目录信息");
            return;
        }

        // 创建导出选项
        var exportOptions = new ExportOptions
        {
            MaxDepth = maxDepth,
            IncludePageNumbers = includePages,
            IncludeLinks = includeLinks,
            CustomTitle = customTitle,
            IndentString = indent,
            PageNumberFormat = pageFormat
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


}
