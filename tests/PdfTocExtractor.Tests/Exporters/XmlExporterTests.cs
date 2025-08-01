using FluentAssertions;
using PdfTocExtractor.Exporters;
using PdfTocExtractor.Models;
using System.Text;
using System.Xml;
using Xunit;

namespace PdfTocExtractor.Tests.Exporters;

public class XmlExporterTests
{
    private readonly XmlExporter _exporter;

    public XmlExporterTests()
    {
        _exporter = new XmlExporter();
    }

    [Fact]
    public void FormatName_ShouldReturnXML()
    {
        // Act & Assert
        _exporter.FormatName.Should().Be("XML");
    }

    [Fact]
    public void FileExtension_ShouldReturnXml()
    {
        // Act & Assert
        _exporter.FileExtension.Should().Be("xml");
    }

    [Fact]
    public void Export_ShouldReturnValidXml_WhenNoTocItems()
    {
        // Arrange
        var tocItems = new List<TocItem>();
        var options = new ExportOptions { CustomTitle = "Test Document" };

        // Act
        var result = _exporter.Export(tocItems, options);

        // Assert
        result.Should().NotBeNullOrEmpty();
        
        var doc = new XmlDocument();
        doc.LoadXml(result);
        
        doc.DocumentElement?.Name.Should().Be("PdfTableOfContents");
        doc.DocumentElement?.GetAttribute("title").Should().Be("Test Document");
        doc.DocumentElement?.ChildNodes.Count.Should().Be(0);
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
        var doc = new XmlDocument();
        doc.LoadXml(result);
        
        doc.DocumentElement?.GetAttribute("title").Should().Be("PDF 目录");
    }

    [Fact]
    public void Export_ShouldCreateCorrectXmlStructure_WithSingleItem()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "5", Level = 0 }
        };

        // Act
        var result = _exporter.Export(tocItems);

        // Assert
        var doc = new XmlDocument();
        doc.LoadXml(result);
        
        var items = doc.DocumentElement?.ChildNodes;
        items?.Count.Should().Be(1);
        
        var item = items?[0] as XmlElement;
        item?.Name.Should().Be("Item");
        item?.GetAttribute("level").Should().Be("0");
        item?.GetAttribute("page").Should().Be("5");
        item?.GetAttribute("pageNumber").Should().Be("5");

        // 检查子元素
        var titleElement = item?.GetElementsByTagName("Title")[0];
        titleElement?.InnerText.Should().Be("Chapter 1");

        var fullPathElement = item?.GetElementsByTagName("FullPath")[0];
        fullPathElement?.InnerText.Should().Be("Chapter 1");
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
        var doc = new XmlDocument();
        doc.LoadXml(result);
        
        var rootItems = doc.DocumentElement?.ChildNodes;
        rootItems?.Count.Should().Be(1);
        
        var chapter = rootItems?[0] as XmlElement;
        var chapterTitle = chapter?.GetElementsByTagName("Title")[0];
        chapterTitle?.InnerText.Should().Be("Chapter 1");

        var childrenContainer = chapter?.GetElementsByTagName("Children")[0];
        childrenContainer.Should().NotBeNull();

        var children = childrenContainer?.ChildNodes;
        children?.Count.Should().Be(2);

        var section1 = children?[0] as XmlElement;
        var section1Title = section1?.GetElementsByTagName("Title")[0];
        section1Title?.InnerText.Should().Be("Section 1.1");
        section1?.GetAttribute("page").Should().Be("6");

        var section2 = children?[1] as XmlElement;
        var section2Title = section2?.GetElementsByTagName("Title")[0];
        section2Title?.InnerText.Should().Be("Section 1.2");
        section2?.GetAttribute("page").Should().Be("10");
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
        var doc = new XmlDocument();
        doc.LoadXml(result);
        
        var chapter = doc.DocumentElement?.ChildNodes[0] as XmlElement;
        var childrenContainer = chapter?.GetElementsByTagName("Children")[0];
        var section = childrenContainer?.ChildNodes[0] as XmlElement;

        var sectionTitle = section?.GetElementsByTagName("Title")[0];
        sectionTitle?.InnerText.Should().Be("Section 1.1");

        // 检查section没有Children元素（因为被maxDepth限制了）
        var sectionChildren = section?.GetElementsByTagName("Children");
        sectionChildren?.Count.Should().Be(0);
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
        var result = _exporter.Export(tocItems);

        // Assert
        var doc = new XmlDocument();
        doc.LoadXml(result);
        
        var item = doc.DocumentElement?.ChildNodes[0] as XmlElement;
        var titleElement = item?.GetElementsByTagName("Title")[0];
        titleElement?.InnerText.Should().Be("Chapter 1");
        item?.HasAttribute("page").Should().BeFalse();
        item?.HasAttribute("pageNumber").Should().BeFalse();
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
        var doc = new XmlDocument();
        doc.LoadXml(result);
        
        var items = doc.DocumentElement?.ChildNodes;
        
        foreach (XmlElement item in items!)
        {
            item.HasAttribute("page").Should().BeFalse();
        }
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
        var doc = new XmlDocument();
        doc.LoadXml(result);
        
        var generated = doc.DocumentElement?.GetAttribute("generated");
        generated.Should().NotBeNullOrEmpty();
        
        DateTime.TryParse(generated, out var timestamp).Should().BeTrue();
        timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Export_ShouldFormatXmlWithIndentation()
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
    public void Export_ShouldEscapeXmlSpecialCharacters()
    {
        // Arrange
        var tocItems = new List<TocItem>
        {
            new TocItem { Title = "Chapter <1> & \"Test\"", Page = "5", Level = 0 }
        };

        // Act
        var result = _exporter.Export(tocItems);

        // Assert
        var doc = new XmlDocument();
        doc.LoadXml(result); // Should not throw exception
        
        var item = doc.DocumentElement?.ChildNodes[0] as XmlElement;
        var titleElement = item?.GetElementsByTagName("Title")[0];
        titleElement?.InnerText.Should().Be("Chapter <1> & \"Test\"");
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
            
            var doc = new XmlDocument();
            doc.LoadXml(content);
            doc.DocumentElement?.GetAttribute("title").Should().Be("Test Export");
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
            
            var doc = new XmlDocument();
            doc.LoadXml(content);
            var item = doc.DocumentElement?.ChildNodes[0] as XmlElement;
            var titleElement = item?.GetElementsByTagName("Title")[0];
            titleElement?.InnerText.Should().Be("测试章节");
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
        
        var doc = new XmlDocument();
        doc.LoadXml(result);
        doc.DocumentElement?.GetAttribute("title").Should().Be("PDF 目录");
    }
}
