using PdfTocExtractor.Exporters;
using PdfTocExtractor.Models;
using Xunit;

namespace PdfTocExtractor.Tests;

public class DebugTest
{
    [Fact]
    public void Debug_XmlExporter_ActualOutput()
    {
        // Arrange
        var exporter = new XmlExporter();
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5", Level = 0 }
        };

        // Act
        try
        {
            System.Console.WriteLine("=== Starting XML Export ===");
            System.Console.WriteLine($"TocItems count: {tocItems.Count}");
            System.Console.WriteLine($"First item: Title='{tocItems[0].Title}', Page='{tocItems[0].Page}', Level={tocItems[0].Level}");

            var result = exporter.Export(tocItems);

            // Assert - 输出实际结果以便调试
            System.Console.WriteLine("=== XML Export Result ===");
            System.Console.WriteLine($"Length: {result.Length}");
            System.Console.WriteLine($"Content: '{result}'");
            System.Console.WriteLine("=== End ===");

            Assert.NotNull(result);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"=== XML Export Exception ===");
            System.Console.WriteLine($"Type: {ex.GetType().Name}");
            System.Console.WriteLine($"Message: {ex.Message}");
            System.Console.WriteLine($"StackTrace: {ex.StackTrace}");
            System.Console.WriteLine("=== End ===");
            throw;
        }
    }

    [Fact]
    public void Debug_XmlExporter_StepByStep()
    {
        // 测试XML导出器的各个步骤
        try
        {
            System.Console.WriteLine("=== Step by Step XML Export ===");

            // Step 1: Create exporter
            var exporter = new XmlExporter();
            System.Console.WriteLine("Step 1: Exporter created");

            // Step 2: Create simple item
            var item = new TocItem { Title = "Test", Page = "1", Level = 0 };
            System.Console.WriteLine($"Step 2: Item created - Title: '{item.Title}'");

            // Step 3: Test GetFullPath
            var fullPath = item.GetFullPath();
            System.Console.WriteLine($"Step 3: GetFullPath result: '{fullPath}'");

            // Step 4: Create list
            var items = new List<TocItem> { item };
            System.Console.WriteLine($"Step 4: List created with {items.Count} items");

            // Step 5: Export
            var result = exporter.Export(items);
            System.Console.WriteLine($"Step 5: Export completed, length: {result.Length}");

            if (result.Length > 0)
            {
                System.Console.WriteLine($"Content: {result}");
            }
            else
            {
                System.Console.WriteLine("Result is empty!");
            }

            System.Console.WriteLine("=== End Step by Step ===");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Exception in step by step: {ex}");
            throw;
        }
    }

    [Fact]
    public void Debug_MarkdownExporter_ActualOutput()
    {
        // Arrange
        var exporter = new MarkdownExporter();
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5", Level = 0 }
        };

        // Act
        var result = exporter.Export(tocItems);

        // Assert - 输出实际结果以便调试
        System.Console.WriteLine("=== Markdown Export Result ===");
        System.Console.WriteLine($"Length: {result.Length}");
        System.Console.WriteLine($"Content: '{result}'");
        System.Console.WriteLine("=== End ===");
        
        Assert.NotNull(result);
    }

    [Fact]
    public void Debug_JsonExporter_ActualOutput()
    {
        // Arrange
        var exporter = new JsonExporter();
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5", Level = 0 }
        };

        // Act
        var result = exporter.Export(tocItems);

        // Assert - 输出实际结果以便调试
        System.Console.WriteLine("=== JSON Export Result ===");
        System.Console.WriteLine($"Length: {result.Length}");
        System.Console.WriteLine($"Content: '{result}'");
        System.Console.WriteLine("=== End ===");
        
        Assert.NotNull(result);
    }
}
