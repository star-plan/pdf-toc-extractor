using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfTocExtractor.Exporters;
using PdfTocExtractor.Models;
using System.Text;
using Xunit;

namespace PdfTocExtractor.Tests.Exporters;

public class JsonExporterTests
{
    private readonly JsonExporter _exporter;

    public JsonExporterTests()
    {
        _exporter = new JsonExporter();
    }

    [Fact]
    public void FormatName_ShouldReturnJSON()
    {
        // Act & Assert
        _exporter.FormatName.Should().Be("JSON");
    }

    [Fact]
    public void FileExtension_ShouldReturnJson()
    {
        // Act & Assert
        _exporter.FileExtension.Should().Be("json");
    }

    [Fact]
    public void Export_ShouldReturnValidJson_WhenNoTocItems()
    {
        // Arrange
        var tocItems = new List<TocItem>();
        var options = new ExportOptions { CustomTitle = "Test Document" };

        // Act
        var result = _exporter.Export(tocItems, options);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var json = JObject.Parse(result);
        json["title"]?.ToString().Should().Be("Test Document");
        json["items"].Should().NotBeNull();
        ((JArray)json["items"]!).Should().BeEmpty();
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
        var json = JObject.Parse(result);
        json["title"]?.ToString().Should().Be("PDF 目录");
    }

    [Fact]
    public void Export_ShouldCreateCorrectJsonStructure_WithSingleItem()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5", Level = 0 }
        };

        // Act
        var result = _exporter.Export(tocItems);

        // Assert
        var json = JObject.Parse(result);
        var items = (JArray)json["items"]!;
        items.Should().HaveCount(1);
        
        var item = (JObject)items[0];
        item["title"]?.ToString().Should().Be("Chapter 1");
        item["level"]?.Value<int>().Should().Be(0);
        item["page"]?.ToString().Should().Be("5");
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
        var json = JObject.Parse(result);
        var items = (JArray)json["items"]!;
        items.Should().HaveCount(1);
        
        var chapter = (JObject)items[0];
        chapter["title"]?.ToString().Should().Be("Chapter 1");
        
        var children = (JArray)chapter["children"]!;
        children.Should().HaveCount(2);
        children[0]["title"]?.ToString().Should().Be("Section 1.1");
        children[1]["title"]?.ToString().Should().Be("Section 1.2");
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
        var json = JObject.Parse(result);
        var items = (JArray)json["items"]!;
        var chapter = (JObject)items[0];
        var children = (JArray)chapter["children"]!;
        
        children.Should().HaveCount(1);
        children[0]["title"]?.ToString().Should().Be("Section 1.1");
        children[0]["children"].Should().BeNull();
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
        var json = JObject.Parse(result);
        var items = (JArray)json["items"]!;
        var item = (JObject)items[0];
        
        item["title"]?.ToString().Should().Be("Chapter 1");
        item["page"].Should().BeNull();
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
        var json = JObject.Parse(result);
        var items = (JArray)json["items"]!;
        
        items.Should().HaveCount(3);
        items[0]["page"].Should().BeNull();
        items[1]["page"].Should().BeNull();
        items[2]["page"].Should().BeNull();
    }

    [Fact]
    public void Export_ShouldIncludeGeneratedTimestamp()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5", Level = 0 }
        };

        // Act
        var result = _exporter.Export(tocItems);

        // Assert
        var json = JObject.Parse(result);
        json["generated"].Should().NotBeNull();
        
        var generated = json["generated"]?.ToString();
        DateTime.TryParse(generated, out var timestamp).Should().BeTrue();
        timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Export_ShouldFormatJsonWithIndentation()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5", Level = 0 }
        };

        // Act
        var result = _exporter.Export(tocItems);

        // Assert
        result.Should().Contain("  "); // Should contain indentation
        result.Should().Contain("\n"); // Should contain line breaks
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
            var json = JObject.Parse(content);
            json["title"]?.ToString().Should().Be("Test Export");
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
            var json = JObject.Parse(content);
            json["items"]?[0]?["title"]?.ToString().Should().Be("测试章节");
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
        var json = JObject.Parse(result);
        json["title"]?.ToString().Should().Be("PDF 目录");
        json["items"].Should().NotBeNull();
    }

    [Fact]
    public void Export_ShouldHandleComplexPageNumbers()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5 XYZ 123 456", Level = 0 }
        };

        // Act
        var result = _exporter.Export(tocItems);

        // Assert
        var json = JObject.Parse(result);
        var items = (JArray)json["items"]!;
        var item = (JObject)items[0];
        
        item["page"]?.ToString().Should().Be("5 XYZ 123 456");
    }
}
