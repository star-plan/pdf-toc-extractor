using System.CommandLine;
using System.CommandLine.Invocation;
using PdfTocExtractor.Exporters;

namespace PdfTocExtractor.Cli.Commands;

public static class ExtractCommand
{
    public static Command Create()
    {
        var inputArgument = new Argument<FileInfo>("input", "PDF文件路径")
        {
            Arity = ArgumentArity.ExactlyOne
        };

        var outputOption = new Option<FileInfo?>(
            aliases: ["-o", "--output"],
            description: "输出文件路径（如果未指定，将使用输入文件名加上相应扩展名）");

        var formatOption = new Option<string?>(
            aliases: ["-f", "--format"],
            description: "输出格式 (markdown, json, xml, text)。如果未指定，将根据输出文件扩展名推断");

        var maxDepthOption = new Option<int>(
            aliases: ["--max-depth"],
            description: "最大层级深度（0表示无限制）",
            getDefaultValue: () => 0);

        var includePageNumbersOption = new Option<bool>(
            aliases: ["--include-pages"],
            description: "是否包含页码信息",
            getDefaultValue: () => true);

        var includeLinksOption = new Option<bool>(
            aliases: ["--include-links"],
            description: "是否包含链接（如果格式支持）",
            getDefaultValue: () => false);

        var customTitleOption = new Option<string?>(
            aliases: ["--title"],
            description: "自定义文档标题");

        var indentOption = new Option<string>(
            aliases: ["--indent"],
            description: "缩进字符串",
            getDefaultValue: () => "  ");

        var pageFormatOption = new Option<string>(
            aliases: ["--page-format"],
            description: "页码格式化字符串",
            getDefaultValue: () => "第 {0} 页");

        var verboseOption = new Option<bool>(
            aliases: ["-v", "--verbose"],
            description: "显示详细输出");

        var command = new Command("extract", "从PDF文件提取目录")
        {
            inputArgument,
            outputOption,
            formatOption,
            maxDepthOption,
            includePageNumbersOption,
            includeLinksOption,
            customTitleOption,
            indentOption,
            pageFormatOption,
            verboseOption
        };

        command.SetHandler(async (context) =>
        {
            try
            {
                var input = context.ParseResult.GetValueForArgument(inputArgument);
                var output = context.ParseResult.GetValueForOption(outputOption);
                var format = context.ParseResult.GetValueForOption(formatOption);
                var maxDepth = context.ParseResult.GetValueForOption(maxDepthOption);
                var includePages = context.ParseResult.GetValueForOption(includePageNumbersOption);
                var includeLinks = context.ParseResult.GetValueForOption(includeLinksOption);
                var customTitle = context.ParseResult.GetValueForOption(customTitleOption);
                var indent = context.ParseResult.GetValueForOption(indentOption) ?? "  ";
                var pageFormat = context.ParseResult.GetValueForOption(pageFormatOption) ?? "第 {0} 页";
                var verbose = context.ParseResult.GetValueForOption(verboseOption);

                await ExecuteExtractCommand(input, output, format, maxDepth, includePages, includeLinks, customTitle, indent, pageFormat, verbose);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"错误: {ex.Message}");
                Console.ResetColor();
                context.ExitCode = 1;
            }
        });

        return command;
    }

    private static async Task ExecuteExtractCommand(
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
            Console.WriteLine($"正在处理PDF文件: {input.FullName}");
        }

        var extractor = new PdfTocExtractor();

        // 提取目录
        var tocItems = await extractor.ExtractTocAsync(input.FullName);

        if (verbose)
        {
            Console.WriteLine($"成功提取 {tocItems.Count} 个顶级目录项");
            var totalItems = tocItems.Sum(item => 1 + item.GetAllDescendants().Count());
            Console.WriteLine($"总共 {totalItems} 个目录项");
        }

        // 确定输出文件和格式
        var (outputFile, outputFormat) = DetermineOutputFileAndFormat(input, output, format);

        if (verbose)
        {
            Console.WriteLine($"输出文件: {outputFile.FullName}");
            Console.WriteLine($"输出格式: {outputFormat}");
        }

        // 配置导出选项
        var exportOptions = new ExportOptions
        {
            MaxDepth = maxDepth,
            IncludePageNumbers = includePages,
            IncludeLinks = includeLinks,
            CustomTitle = customTitle,
            IndentString = indent,
            PageNumberFormat = pageFormat
        };

        // 导出
        await extractor.ExportToFileAsync(tocItems, outputFile.FullName, outputFormat, exportOptions);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ 成功导出到: {outputFile.FullName}");
        Console.ResetColor();

        if (verbose)
        {
            Console.WriteLine($"文件大小: {new FileInfo(outputFile.FullName).Length} 字节");
        }
    }

    private static (FileInfo outputFile, string format) DetermineOutputFileAndFormat(FileInfo input, FileInfo? output, string? format)
    {
        if (output != null)
        {
            // 如果指定了输出文件
            var outputFormat = format ?? Path.GetExtension(output.FullName).TrimStart('.');
            if (string.IsNullOrEmpty(outputFormat))
            {
                outputFormat = "md"; // 默认为markdown
            }
            return (output, outputFormat);
        }
        else
        {
            // 如果没有指定输出文件，根据格式生成
            var outputFormat = format ?? "md";
            var extension = outputFormat switch
            {
                "markdown" or "md" => "md",
                "json" => "json",
                "xml" => "xml",
                "text" or "txt" => "txt",
                _ => outputFormat
            };
            
            var outputPath = Path.ChangeExtension(input.FullName, extension);
            return (new FileInfo(outputPath), outputFormat);
        }
    }
}
