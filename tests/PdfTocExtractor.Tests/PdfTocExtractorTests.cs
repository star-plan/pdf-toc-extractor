using FluentAssertions;
using Moq;
using PdfTocExtractor.Exporters;
using PdfTocExtractor.Models;
using PdfTocExtractor.Tests.TestData;
using Xunit;

namespace PdfTocExtractor.Tests;

public class PdfTocExtractorTests : IDisposable
{
    private readonly PdfTocExtractor _extractor;
    private readonly List<string> _tempFiles;

    public PdfTocExtractorTests()
    {
        _extractor = new PdfTocExtractor();
        _tempFiles = new List<string>();
    }

    public void Dispose()
    {
        // 清理临时文件
        foreach (var file in _tempFiles)
        {
            MockHelpers.CleanupTempFile(file);
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultExporters()
    {
        // Arrange & Act
        var extractor = new PdfTocExtractor();

        // Assert
        var supportedFormats = extractor.GetSupportedFormats().ToList();
        supportedFormats.Should().Contain("markdown");
        supportedFormats.Should().Contain("md");
        supportedFormats.Should().Contain("json");
        supportedFormats.Should().Contain("xml");
        supportedFormats.Should().Contain("text");
        supportedFormats.Should().Contain("txt");
    }

    [Fact]
    public void GetSupportedFormats_ShouldReturnAllRegisteredFormats()
    {
        // Act
        var formats = _extractor.GetSupportedFormats().ToList();

        // Assert
        formats.Should().HaveCountGreaterThan(0);
        formats.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void RegisterExporter_ShouldAddNewExporter()
    {
        // Arrange
        var mockExporter = MockHelpers.CreateMockExporter("custom", "cst");

        // Act
        _extractor.RegisterExporter("custom", mockExporter.Object);

        // Assert
        var formats = _extractor.GetSupportedFormats();
        formats.Should().Contain("custom");
    }

    [Fact]
    public void RegisterExporter_ShouldReplaceExistingExporter()
    {
        // Arrange
        var mockExporter = MockHelpers.CreateMockExporter("markdown", "md");

        // Act
        _extractor.RegisterExporter("markdown", mockExporter.Object);

        // Assert
        // 应该能够成功注册，不抛出异常
        var formats = _extractor.GetSupportedFormats();
        formats.Should().Contain("markdown");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void RegisterExporter_ShouldThrowArgumentException_WhenFormatIsNullOrEmpty(string format)
    {
        // Arrange
        var mockExporter = MockHelpers.CreateMockExporter();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _extractor.RegisterExporter(format, mockExporter.Object));
    }

    [Fact]
    public void RegisterExporter_ShouldThrowArgumentNullException_WhenExporterIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _extractor.RegisterExporter("test", null!));
    }

    [Fact]
    public void ExtractToc_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentPath = MockHelpers.CreateNonExistentPdfPath();

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => _extractor.ExtractToc(nonExistentPath));
    }

    [Fact]
    public async Task ExtractTocAsync_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentPath = MockHelpers.CreateNonExistentPdfPath();

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _extractor.ExtractTocAsync(nonExistentPath));
    }

    [Fact]
    public void ExportToString_ShouldReturnCorrectFormat_WithMarkdown()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSimpleTocItems();

        // Act
        var result = _extractor.ExportToString(tocItems, "markdown");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("# PDF 目录");
        result.Should().Contain("- Chapter 1（第 1 页）");
    }

    [Fact]
    public void ExportToString_ShouldReturnCorrectFormat_WithJson()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSimpleTocItems();

        // Act
        var result = _extractor.ExportToString(tocItems, "json");

        // Assert
        result.Should().NotBeNullOrEmpty();
        MockHelpers.IsValidJson(result).Should().BeTrue();
    }

    [Fact]
    public void ExportToString_ShouldReturnCorrectFormat_WithXml()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSimpleTocItems();

        // Act
        var result = _extractor.ExportToString(tocItems, "xml");

        // Assert
        result.Should().NotBeNullOrEmpty();
        MockHelpers.IsValidXml(result).Should().BeTrue();
    }

    [Fact]
    public void ExportToString_ShouldReturnCorrectFormat_WithText()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSimpleTocItems();

        // Act
        var result = _extractor.ExportToString(tocItems, "text");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("PDF 目录");
        result.Should().Contain("- Chapter 1（第 1 页）");
    }

    [Fact]
    public void ExportToString_ShouldThrowArgumentException_WhenFormatNotSupported()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSimpleTocItems();

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _extractor.ExportToString(tocItems, "unsupported"));
    }

    [Fact]
    public void ExportToString_ShouldUseOptions_WhenProvided()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSimpleTocItems();
        var options = MockHelpers.CreateTestExportOptions(customTitle: "Custom Title");

        // Act
        var result = _extractor.ExportToString(tocItems, "markdown", options);

        // Assert
        result.Should().Contain("# Custom Title");
    }

    [Fact]
    public async Task ExportToFileAsync_ShouldCreateFile_WithCorrectContent()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSimpleTocItems();
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);

        // Act
        await _extractor.ExportToFileAsync(tocItems, tempFile, "markdown");

        // Assert
        File.Exists(tempFile).Should().BeTrue();
        var content = await File.ReadAllTextAsync(tempFile);
        content.Should().Contain("# PDF 目录");
    }

    [Fact]
    public async Task ExportToFileAsync_ShouldInferFormat_FromFileExtension()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSimpleTocItems();
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".json");
        _tempFiles.Add(tempFile);

        // Act
        await _extractor.ExportToFileAsync(tocItems, tempFile);

        // Assert
        File.Exists(tempFile).Should().BeTrue();
        var content = await File.ReadAllTextAsync(tempFile);
        MockHelpers.IsValidJson(content).Should().BeTrue();
    }

    [Fact]
    public async Task ExportToFileAsync_ShouldUseSpecifiedFormat_OverFileExtension()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSimpleTocItems();
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".json");
        _tempFiles.Add(tempFile);

        // Act
        await _extractor.ExportToFileAsync(tocItems, tempFile, "markdown");

        // Assert
        File.Exists(tempFile).Should().BeTrue();
        var content = await File.ReadAllTextAsync(tempFile);
        content.Should().Contain("# PDF 目录"); // Markdown format, not JSON
    }

    [Fact]
    public async Task ExportToFileAsync_ShouldThrowArgumentException_WhenFormatNotSupported()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSimpleTocItems();
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            _extractor.ExportToFileAsync(tocItems, tempFile, "unsupported"));
    }

    [Fact]
    public async Task ExtractAndExportAsync_ShouldThrowFileNotFoundException_WhenPdfNotExists()
    {
        // Arrange
        var nonExistentPdf = MockHelpers.CreateNonExistentPdfPath();
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => 
            _extractor.ExtractAndExportAsync(nonExistentPdf, tempFile));
    }

    [Theory]
    [InlineData("markdown")]
    [InlineData("md")]
    [InlineData("json")]
    [InlineData("xml")]
    [InlineData("text")]
    [InlineData("txt")]
    public void ExportToString_ShouldWorkWithAllSupportedFormats(string format)
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSimpleTocItems();

        // Act
        var result = _extractor.ExportToString(tocItems, format);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ExportToString_ShouldHandleEmptyTocItems()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateEmptyTocItems();

        // Act
        var result = _extractor.ExportToString(tocItems, "markdown");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("# PDF 目录");
    }

    [Fact]
    public void ExportToString_ShouldHandleHierarchicalTocItems()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateHierarchicalTocItems();

        // Act
        var result = _extractor.ExportToString(tocItems, "markdown");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Chapter 1: Introduction");
        result.Should().Contain("1.1 Overview");
        result.Should().Contain("1.2.1 Primary Goals");
    }

    [Fact]
    public void ExportToString_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var tocItems = TestDataBuilder.CreateSpecialCharacterTocItems();

        // Act
        var result = _extractor.ExportToString(tocItems, "markdown");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Chapter <1> & \"Introduction\"");
    }
}
