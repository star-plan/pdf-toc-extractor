using FluentAssertions;
using PdfTocExtractor.Exporters;
using PdfTocExtractor.Tests.TestData;
using Xunit;

namespace PdfTocExtractor.Tests.Integration;

/// <summary>
/// 集成测试类 - 测试完整的工作流程
/// </summary>
public class IntegrationTests : IDisposable
{
    private readonly PdfTocExtractor _extractor;
    private readonly List<string> _tempFiles;
    private readonly List<string> _tempDirectories;

    public IntegrationTests()
    {
        _extractor = new PdfTocExtractor();
        _tempFiles = new List<string>();
        _tempDirectories = new List<string>();
    }

    public void Dispose()
    {
        // 清理临时文件和目录
        foreach (var file in _tempFiles)
        {
            MockHelpers.CleanupTempFile(file);
        }
        foreach (var dir in _tempDirectories)
        {
            MockHelpers.CleanupTempDirectory(dir);
        }
    }

    [Fact]
    public void CompleteWorkflow_ShouldWork_WithAllExporters()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateHierarchicalTocItems();
        var supportedFormats = _extractor.GetSupportedFormats().ToList();

        // Act & Assert
        foreach (var format in supportedFormats)
        {
            // 测试字符串导出
            var stringResult = _extractor.ExportToString(tocItems, format);
            stringResult.Should().NotBeNullOrEmpty($"Format {format} should produce non-empty output");

            // 测试文件导出
            var tempFile = Path.GetTempFileName();
            _tempFiles.Add(tempFile);

            var exportTask = _extractor.ExportToFileAsync(tocItems, tempFile, format);
            exportTask.Should().NotBeNull();
            exportTask.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue($"Export to {format} should complete within 10 seconds");

            File.Exists(tempFile).Should().BeTrue($"File should be created for format {format}");
            var fileInfo = new FileInfo(tempFile);
            fileInfo.Length.Should().BeGreaterThan(0, $"File should not be empty for format {format}");
        }
    }

    [Fact]
    public async Task ExportToMultipleFormats_ShouldProduceConsistentResults()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateHierarchicalTocItems();
        var outputDir = MockHelpers.CreateTempDirectory();
        _tempDirectories.Add(outputDir);

        var formats = new[] { "markdown", "json", "xml", "text" };
        var options = MockHelpers.CreateTestExportOptions(customTitle: "Integration Test Document");

        // Act
        var exportTasks = formats.Select(async format =>
        {
            var fileName = $"test.{_extractor.GetSupportedFormats().First(f => f.Equals(format, StringComparison.OrdinalIgnoreCase))}";
            var filePath = Path.Combine(outputDir, fileName);
            await _extractor.ExportToFileAsync(tocItems, filePath, format, options);
            return new { Format = format, FilePath = filePath };
        });

        var results = await Task.WhenAll(exportTasks);

        // Assert
        foreach (var result in results)
        {
            File.Exists(result.FilePath).Should().BeTrue($"File should exist for {result.Format}");
            
            var content = await File.ReadAllTextAsync(result.FilePath);
            content.Should().NotBeNullOrEmpty($"Content should not be empty for {result.Format}");
            content.Should().Contain("Integration Test Document", $"Custom title should appear in {result.Format}");
            
            // 验证内容包含预期的章节
            content.Should().Contain("Chapter 1: Introduction", $"Chapter 1 should appear in {result.Format}");
            content.Should().Contain("Chapter 2: Methodology", $"Chapter 2 should appear in {result.Format}");
        }
    }

    [Fact]
    public async Task ExportWithDifferentOptions_ShouldProduceExpectedVariations()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateDeepNestedTocItems();
        var baseDir = MockHelpers.CreateTempDirectory();
        _tempDirectories.Add(baseDir);

        var testCases = new[]
        {
            new { Name = "default", Options = new ExportOptions() },
            new { Name = "no_pages", Options = new ExportOptions { IncludePageNumbers = false } },
            new { Name = "max_depth_2", Options = new ExportOptions { MaxDepth = 2 } },
            new { Name = "custom_indent", Options = new ExportOptions { IndentString = "\t" } },
            new { Name = "custom_format", Options = new ExportOptions { PageNumberFormat = "Page {0}" } }
        };

        // Act & Assert
        foreach (var testCase in testCases)
        {
            var filePath = Path.Combine(baseDir, $"{testCase.Name}.md");
            await _extractor.ExportToFileAsync(tocItems, filePath, "markdown", testCase.Options);

            File.Exists(filePath).Should().BeTrue($"File should exist for {testCase.Name}");
            var content = await File.ReadAllTextAsync(filePath);

            switch (testCase.Name)
            {
                case "no_pages":
                    content.Should().NotContain("第", "Should not contain page numbers");
                    break;
                case "max_depth_2":
                    content.Should().NotContain("Level 3", "Should not contain deep nested items");
                    break;
                case "custom_indent":
                    content.Should().Contain("\t", "Should contain tab indentation");
                    break;
                case "custom_format":
                    content.Should().Contain("Page ", "Should contain custom page format");
                    break;
            }
        }
    }

    [Fact]
    public async Task LargeDataSet_ShouldBeHandledEfficiently()
    {
        // Arrange
        var largeTocItems = TestDataBuilder.CreateLargeTocItems(50); // 50个章节，每5个有子节
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        await _extractor.ExportToFileAsync(largeTocItems, tempFile, "json");
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "Large dataset export should complete within 5 seconds");
        
        File.Exists(tempFile).Should().BeTrue();
        var fileInfo = new FileInfo(tempFile);
        fileInfo.Length.Should().BeGreaterThan(1000, "Large dataset should produce substantial output");

        var content = await File.ReadAllTextAsync(tempFile);
        MockHelpers.IsValidJson(content).Should().BeTrue("Output should be valid JSON");
    }

    [Fact]
    public async Task CustomExporter_ShouldIntegrateCorrectly()
    {
        // Arrange
        var customExporter = new CustomTestExporter();
        _extractor.RegisterExporter("custom", customExporter);

        var tocItems = TestDataBuilder.CreateSimpleTocItems();
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);

        // Act
        await _extractor.ExportToFileAsync(tocItems, tempFile, "custom");

        // Assert
        File.Exists(tempFile).Should().BeTrue();
        var content = await File.ReadAllTextAsync(tempFile);
        content.Should().Contain("CUSTOM FORMAT");
        content.Should().Contain("Chapter 1");
    }

    [Fact]
    public void SpecialCharacters_ShouldBeHandledCorrectly_InAllFormats()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSpecialCharacterTocItems();
        var formats = new[] { "markdown", "json", "xml", "text" };

        // Act & Assert
        foreach (var format in formats)
        {
            var result = _extractor.ExportToString(tocItems, format);
            
            result.Should().NotBeNullOrEmpty($"Format {format} should handle special characters");
            
            // 验证特殊字符被正确处理
            switch (format.ToLower())
            {
                case "json":
                    MockHelpers.IsValidJson(result).Should().BeTrue($"JSON with special characters should be valid");
                    break;
                case "xml":
                    MockHelpers.IsValidXml(result).Should().BeTrue($"XML with special characters should be valid");
                    break;
            }
        }
    }

    [Fact]
    public async Task FileExtensionInference_ShouldWorkCorrectly()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSimpleTocItems();
        var baseDir = MockHelpers.CreateTempDirectory();
        _tempDirectories.Add(baseDir);

        var testFiles = new[]
        {
            Path.Combine(baseDir, "test.md"),
            Path.Combine(baseDir, "test.markdown"),
            Path.Combine(baseDir, "test.json"),
            Path.Combine(baseDir, "test.xml"),
            Path.Combine(baseDir, "test.txt")
        };

        // Act
        foreach (var file in testFiles)
        {
            await _extractor.ExportToFileAsync(tocItems, file); // 不指定格式，让其自动推断
        }

        // Assert
        foreach (var file in testFiles)
        {
            File.Exists(file).Should().BeTrue($"File {file} should be created");
            var content = await File.ReadAllTextAsync(file);
            content.Should().NotBeNullOrEmpty($"File {file} should have content");

            var extension = Path.GetExtension(file).TrimStart('.');
            switch (extension.ToLower())
            {
                case "json":
                    MockHelpers.IsValidJson(content).Should().BeTrue($"JSON file should be valid");
                    break;
                case "xml":
                    MockHelpers.IsValidXml(content).Should().BeTrue($"XML file should be valid");
                    break;
                case "md":
                case "markdown":
                    content.Should().Contain("# PDF 目录", "Markdown should have header");
                    break;
                case "txt":
                    content.Should().Contain("PDF 目录", "Text should have title");
                    break;
            }
        }
    }

    [Fact]
    public async Task ConcurrentExports_ShouldWorkCorrectly()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateHierarchicalTocItems();
        var baseDir = MockHelpers.CreateTempDirectory();
        _tempDirectories.Add(baseDir);

        var concurrentTasks = Enumerable.Range(1, 10).Select(async i =>
        {
            var filePath = Path.Combine(baseDir, $"concurrent_{i}.json");
            await _extractor.ExportToFileAsync(tocItems, filePath, "json");
            return filePath;
        });

        // Act
        var results = await Task.WhenAll(concurrentTasks);

        // Assert
        foreach (var filePath in results)
        {
            File.Exists(filePath).Should().BeTrue($"Concurrent export file {filePath} should exist");
            var content = await File.ReadAllTextAsync(filePath);
            MockHelpers.IsValidJson(content).Should().BeTrue($"Concurrent export {filePath} should produce valid JSON");
        }
    }

    /// <summary>
    /// 自定义测试导出器
    /// </summary>
    private class CustomTestExporter : IExporter
    {
        public string FormatName => "Custom Test";
        public string FileExtension => "custom";

        public string Export(IEnumerable<TocItem> tocItems, ExportOptions? options = null)
        {
            var result = "CUSTOM FORMAT\n";
            foreach (var item in tocItems)
            {
                result += $"- {item.Title}\n";
            }
            return result;
        }

        public async Task ExportToFileAsync(IEnumerable<TocItem> tocItems, string filePath, ExportOptions? options = null)
        {
            var content = Export(tocItems, options);
            await File.WriteAllTextAsync(filePath, content);
        }
    }
}
