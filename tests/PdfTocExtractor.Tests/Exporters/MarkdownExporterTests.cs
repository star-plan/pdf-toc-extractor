using FluentAssertions;
using PdfTocExtractor.Exporters;
using PdfTocExtractor.Models;
using System.Text;
using Xunit;

namespace PdfTocExtractor.Tests.Exporters;

public class MarkdownExporterTests
{
    private readonly MarkdownExporter _exporter;

    public MarkdownExporterTests()
    {
        _exporter = new MarkdownExporter();
    }

    [Fact]
    public void FormatName_ShouldReturnMarkdown()
    {
        // Act & Assert
        _exporter.FormatName.Should().Be("Markdown");
    }

    [Fact]
    public void FileExtension_ShouldReturnMd()
    {
        // Act & Assert
        _exporter.FileExtension.Should().Be("md");
    }

    [Fact]
    public void Export_ShouldReturnEmptyDocumentWithTitle_WhenNoTocItems()
    {
        // Arrange
        var tocItems = new List<TocItem>();
        var options = new ExportOptions { CustomTitle = "Test Document" };

        // Act
        var result = _exporter.Export(tocItems, options);

        // Assert
        result.Should().Contain("# Test Document");
        result.Should().NotContain("##"); // No sub-headers
    }

    [Fact]
    public void Export_ShouldUseDefaultTitle_WhenCustomTitleIsNull()
    {
        // Arrange
        var tocItems = new List<TocItem>();
        var options = new ExportOptions { CustomTitle = null };

        // Act
        var result = _exporter.Export(tocItems, options);

        // Assert
        result.Should().Contain("# PDF 目录");
    }

    [Fact]
    public void Export_ShouldCreateCorrectMarkdownStructure_WithSingleItem()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5", Level = 0 }
        };

        // Act
        var result = _exporter.Export(tocItems);

        // Assert
        result.Should().Contain("# PDF 目录");
        result.Should().Contain("- Chapter 1 (第 5 页)");
    }

    [Fact]
    public void Export_ShouldCreateCorrectHierarchy_WithNestedItems()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem 
            { 
                Title = "Chapter 1", 
                Page = "5", 
                Level = 0,
                Children = new List<TocItem>
                {
                    new TocItem { Title = "Section 1.1", Page = "6", Level = 1 },
                    new TocItem { Title = "Section 1.2", Page = "10", Level = 1 }
                }
            }
        };

        // Act
        var result = _exporter.Export(tocItems);

        // Assert
        result.Should().Contain("- Chapter 1 (第 5 页)");
        result.Should().Contain("  - Section 1.1 (第 6 页)");
        result.Should().Contain("  - Section 1.2 (第 10 页)");
    }

    [Fact]
    public void Export_ShouldRespectMaxDepth_WhenSpecified()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem 
            { 
                Title = "Chapter 1", 
                Page = "5", 
                Level = 0,
                Children = new List<TocItem>
                {
                    new TocItem 
                    { 
                        Title = "Section 1.1", 
                        Page = "6", 
                        Level = 1,
                        Children = new List<TocItem>
                        {
                            new TocItem { Title = "Subsection 1.1.1", Page = "7", Level = 2 }
                        }
                    }
                }
            }
        };
        var options = new ExportOptions { MaxDepth = 1 };

        // Act
        var result = _exporter.Export(tocItems, options);

        // Assert
        result.Should().Contain("- Chapter 1 (第 5 页)");
        result.Should().Contain("  - Section 1.1 (第 6 页)");
        result.Should().NotContain("Subsection 1.1.1");
    }

    [Fact]
    public void Export_ShouldExcludePageNumbers_WhenIncludePageNumbersIsFalse()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5", Level = 0 }
        };
        var options = new ExportOptions { IncludePageNumbers = false };

        // Act
        var result = _exporter.Export(tocItems, options);

        // Assert
        result.Should().Contain("- Chapter 1");
        result.Should().NotContain("第 5 页");
        result.Should().NotContain("(");
        result.Should().NotContain(")");
    }

    [Fact]
    public void Export_ShouldUseCustomIndentation_WhenSpecified()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem 
            { 
                Title = "Chapter 1", 
                Page = "5", 
                Level = 0,
                Children = new List<TocItem>
                {
                    new TocItem { Title = "Section 1.1", Page = "6", Level = 1 }
                }
            }
        };
        var options = new ExportOptions { IndentString = "\t" };

        // Act
        var result = _exporter.Export(tocItems, options);

        // Assert
        result.Should().Contain("- Chapter 1 (第 5 页)");
        result.Should().Contain("\t- Section 1.1 (第 6 页)");
    }

    [Fact]
    public void Export_ShouldUseCustomPageNumberFormat_WhenSpecified()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5", Level = 0 }
        };
        var options = new ExportOptions { PageNumberFormat = "Page {0}" };

        // Act
        var result = _exporter.Export(tocItems, options);

        // Assert
        result.Should().Contain("- Chapter 1 (Page 5)");
    }

    [Fact]
    public void Export_ShouldHandleEmptyPageNumbers()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "", Level = 0 },
            new TocItem { Title = "Chapter 2", Page = "无页码", Level = 0 },
            new TocItem { Title = "Chapter 3", Page = "N/A", Level = 0 }
        };

        // Act
        var result = _exporter.Export(tocItems);

        // Assert
        result.Should().Contain("- Chapter 1");
        result.Should().Contain("- Chapter 2");
        result.Should().Contain("- Chapter 3");
        result.Should().NotContain("第  页");
        result.Should().NotContain("第 无页码 页");
        result.Should().NotContain("第 N/A 页");
    }

    [Fact]
    public async Task ExportToFileAsync_ShouldCreateFileWithCorrectContent()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5", Level = 0 }
        };
        var tempFile = Path.GetTempFileName();
        var options = new ExportOptions { CustomTitle = "Test Export" };

        try
        {
            // Act
            await _exporter.ExportToFileAsync(tocItems, tempFile, options);

            // Assert
            File.Exists(tempFile).Should().BeTrue();
            var content = await File.ReadAllTextAsync(tempFile);
            content.Should().Contain("# Test Export");
            content.Should().Contain("- Chapter 1 (第 5 页)");
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ExportToFileAsync_ShouldUseSpecifiedEncoding()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "测试章节", Page = "5", Level = 0 }
        };
        var tempFile = Path.GetTempFileName();
        var options = new ExportOptions { Encoding = Encoding.Unicode };

        try
        {
            // Act
            await _exporter.ExportToFileAsync(tocItems, tempFile, options);

            // Assert
            File.Exists(tempFile).Should().BeTrue();
            var content = await File.ReadAllTextAsync(tempFile, Encoding.Unicode);
            content.Should().Contain("测试章节");
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Export_ShouldHandleNullOptions()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5", Level = 0 }
        };

        // Act
        var result = _exporter.Export(tocItems, null);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("# PDF 目录");
        result.Should().Contain("- Chapter 1 (第 5 页)");
    }
}
