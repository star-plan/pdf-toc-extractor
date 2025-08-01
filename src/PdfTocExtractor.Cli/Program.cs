using System.CommandLine;
using PdfTocExtractor.Cli.Commands;

namespace PdfTocExtractor.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("PDF Table of Contents Extractor - 从PDF文件提取目录并导出为多种格式")
        {
            ExtractCommand.Create()
        };

        rootCommand.SetHandler(() =>
        {
            Console.WriteLine("PDF Table of Contents Extractor");
            Console.WriteLine("使用 --help 查看可用命令");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  pdftoc extract input.pdf -o output.md");
            Console.WriteLine("  pdftoc extract input.pdf -o output.json -f json");
            Console.WriteLine("  pdftoc extract input.pdf -o output.xml --max-depth 3");
        });

        return await rootCommand.InvokeAsync(args);
    }
}
