using System.CommandLine;
using System.Text;
using iText.Kernel.Pdf;

namespace PdfTocExtractor.Cli.Commands;

public static class DiagnoseCommand
{
    public static Command Create()
    {
        var inputArgument = new Argument<FileInfo>("input", "PDF文件路径")
        {
            Arity = ArgumentArity.ExactlyOne
        };

        var command = new Command("diagnose", "诊断PDF文件的详细信息")
        {
            inputArgument
        };

        command.SetHandler(async (context) =>
        {
            try
            {
                var input = context.ParseResult.GetValueForArgument(inputArgument);
                await ExecuteDiagnoseCommand(input);
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

    private static async Task ExecuteDiagnoseCommand(FileInfo input)
    {
        if (!input.Exists)
        {
            throw new FileNotFoundException($"输入文件不存在: {input.FullName}");
        }

        Console.WriteLine("=== PDF文件诊断信息 ===");
        Console.WriteLine($"文件路径: {input.FullName}");
        Console.WriteLine($"文件大小: {input.Length} 字节");
        Console.WriteLine();

        // 基本文件信息
        await DiagnoseFileBasics(input);
        
        // PDF结构信息
        await DiagnosePdfStructure(input);
        
        // 尝试不同的读取方式
        await TryDifferentReadingMethods(input);
    }

    private static async Task DiagnoseFileBasics(FileInfo input)
    {
        Console.WriteLine("=== 基本文件信息 ===");

        try
        {
            // 读取文件头
            using var fs = new FileStream(input.FullName, FileMode.Open, FileAccess.Read);
            var header = new byte[8];
            await fs.ReadAsync(header, 0, 8);
            
            var headerString = Encoding.ASCII.GetString(header);
            Console.WriteLine($"文件头: {headerString}");
            Console.WriteLine($"文件头(十六进制): {Convert.ToHexString(header)}");
            
            if (headerString.StartsWith("%PDF-"))
            {
                Console.WriteLine("✓ 文件头格式正确");
            }
            else
            {
                Console.WriteLine("✗ 文件头格式不正确，可能不是有效的PDF文件");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ 读取文件头失败: {ex.Message}");
        }
        
        Console.WriteLine();
    }

    private static Task DiagnosePdfStructure(FileInfo input)
    {
        Console.WriteLine("=== PDF结构信息 ===");
        
        try
        {
            using var reader = new PdfReader(input.FullName);
            Console.WriteLine("✓ PdfReader创建成功");

            // 尝试创建PdfDocument来获取更多信息
            try
            {
                using var pdfDoc = new PdfDocument(reader);
                Console.WriteLine("✓ PdfDocument创建成功");

                // 现在可以安全地检查加密状态
                Console.WriteLine($"是否加密: {reader.IsEncrypted()}");

                var pageCount = pdfDoc.GetNumberOfPages();
                Console.WriteLine($"页面数量: {pageCount}");

                var outlines = pdfDoc.GetOutlines(false);
                if (outlines != null)
                {
                    var bookmarks = outlines.GetAllChildren();
                    Console.WriteLine($"书签数量: {bookmarks?.Count ?? 0}");

                    if (bookmarks != null && bookmarks.Count > 0)
                    {
                        Console.WriteLine("✓ 找到书签信息，应该可以提取目录");
                    }
                    else
                    {
                        Console.WriteLine("⚠️  没有书签信息，无法提取目录");
                    }
                }
                else
                {
                    Console.WriteLine("⚠️  没有书签信息，无法提取目录");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 创建PdfDocument失败: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine("这通常表示PDF有加密或权限保护");

                // 尝试获取更多错误详情
                if (ex.Message.Contains("PdfEncryption"))
                {
                    Console.WriteLine("⚠️  这是一个加密相关的错误");
                    Console.WriteLine("可能的原因：");
                    Console.WriteLine("1. PDF设置了权限密码（owner password）");
                    Console.WriteLine("2. PDF使用了不支持的加密算法");
                    Console.WriteLine("3. PDF文件可能需要特定的解密方式");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ PDF结构分析失败: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"详细错误: {ex}");
        }
        
        Console.WriteLine();
        return Task.CompletedTask;
    }

    private static Task TryDifferentReadingMethods(FileInfo input)
    {
        Console.WriteLine("=== 尝试不同的读取方式 ===");
        
        // 方法1: 标准读取
        Console.WriteLine("1. 标准读取方式:");
        try
        {
            using var reader = new PdfReader(input.FullName);
            using var pdfDoc = new PdfDocument(reader);
            Console.WriteLine("✓ 标准读取成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ 标准读取失败: {ex.GetType().Name}: {ex.Message}");
        }
        
        // 方法2: 使用空密码
        Console.WriteLine("2. 使用空密码:");
        try
        {
            var readerProperties = new ReaderProperties().SetPassword(new byte[0]);
            using var reader = new PdfReader(input.FullName, readerProperties);
            using var pdfDoc = new PdfDocument(reader);
            Console.WriteLine("✓ 空密码读取成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ 空密码读取失败: {ex.GetType().Name}: {ex.Message}");
        }

        // 方法2.5: 使用null密码
        Console.WriteLine("2.5. 使用null密码:");
        try
        {
            var readerProperties = new ReaderProperties().SetPassword(null);
            using var reader = new PdfReader(input.FullName, readerProperties);
            using var pdfDoc = new PdfDocument(reader);
            Console.WriteLine("✓ null密码读取成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ null密码读取失败: {ex.GetType().Name}: {ex.Message}");
        }
        
        // 方法3: 使用SetUnethicalReading
        Console.WriteLine("3. 使用SetUnethicalReading:");
        try
        {
            using var reader = new PdfReader(input.FullName).SetUnethicalReading(true);
            using var pdfDoc = new PdfDocument(reader);
            Console.WriteLine("✓ SetUnethicalReading读取成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ SetUnethicalReading读取失败: {ex.GetType().Name}: {ex.Message}");
        }
        
        Console.WriteLine();
        return Task.CompletedTask;
    }
}
