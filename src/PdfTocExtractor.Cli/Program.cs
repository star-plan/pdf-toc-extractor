using System.CommandLine;
using PdfTocExtractor.Cli.Commands;

namespace PdfTocExtractor.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("PDF Table of Contents Extractor - 从PDF文件提取目录并导出为多种格式")
        {
            ExtractCommand.Create(),
            SmartCommand.Create(),
            SemanticCommand.Create(),
            DiagnoseCommand.Create()
        };

        rootCommand.SetHandler(() =>
        {
            Console.WriteLine("PDF Table of Contents Extractor");
            Console.WriteLine("使用 --help 查看可用命令");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  pdftoc extract input.pdf -o output.md     # 提取PDF书签");
            Console.WriteLine("  pdftoc semantic input.pdf -o output.md    # 语义分析提取");
            Console.WriteLine("  pdftoc smart input.pdf -o output.md       # 智能提取（推荐）");
            Console.WriteLine("  pdftoc extract input.pdf -o output.json -f json");
            Console.WriteLine("  pdftoc semantic input.pdf --mode strict --debug # 严格模式+调试");
            Console.WriteLine("  pdftoc diagnose input.pdf                 # 诊断PDF文件问题");
        });

        return await rootCommand.InvokeAsync(args);
    }
}
