using FluentAssertions;
using PdfTocExtractor.Exporters;
using System.Text;
using Xunit;

namespace PdfTocExtractor.Tests.Exporters;

public class ExportOptionsTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var options = new ExportOptions();

        // Assert
        options.IndentString.Should().Be("  ");
        options.IncludePageNumbers.Should().BeTrue();
        options.IncludeLinks.Should().BeFalse();
        options.MaxDepth.Should().Be(0);
        options.PageNumberFormat.Should().Be("第 {0} 页");
        options.CustomTitle.Should().BeNull();
        options.Encoding.Should().Be(Encoding.UTF8);
    }

    [Fact]
    public void IndentString_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var options = new ExportOptions();
        var customIndent = "    "; // 4 spaces

        // Act
        options.IndentString = customIndent;

        // Assert
        options.IndentString.Should().Be(customIndent);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IncludePageNumbers_ShouldSetAndGetCorrectly(bool value)
    {
        // Arrange
        var options = new ExportOptions();

        // Act
        options.IncludePageNumbers = value;

        // Assert
        options.IncludePageNumbers.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IncludeLinks_ShouldSetAndGetCorrectly(bool value)
    {
        // Arrange
        var options = new ExportOptions();

        // Act
        options.IncludeLinks = value;

        // Assert
        options.IncludeLinks.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    public void MaxDepth_ShouldSetAndGetCorrectly(int depth)
    {
        // Arrange
        var options = new ExportOptions();

        // Act
        options.MaxDepth = depth;

        // Assert
        options.MaxDepth.Should().Be(depth);
    }

    [Theory]
    [InlineData("Page {0}")]
    [InlineData("第 {0} 页")]
    [InlineData("p. {0}")]
    [InlineData("{0}")]
    public void PageNumberFormat_ShouldSetAndGetCorrectly(string format)
    {
        // Arrange
        var options = new ExportOptions();

        // Act
        options.PageNumberFormat = format;

        // Assert
        options.PageNumberFormat.Should().Be(format);
    }

    [Theory]
    [InlineData("Custom Title")]
    [InlineData("文档目录")]
    [InlineData("")]
    [InlineData(null)]
    public void CustomTitle_ShouldSetAndGetCorrectly(string? title)
    {
        // Arrange
        var options = new ExportOptions();

        // Act
        options.CustomTitle = title;

        // Assert
        options.CustomTitle.Should().Be(title);
    }

    [Fact]
    public void Encoding_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var options = new ExportOptions();
        var encoding = Encoding.ASCII;

        // Act
        options.Encoding = encoding;

        // Assert
        options.Encoding.Should().Be(encoding);
    }

    [Fact]
    public void AllProperties_ShouldBeSettableIndependently()
    {
        // Arrange
        var options = new ExportOptions();

        // Act
        options.IndentString = "\t";
        options.IncludePageNumbers = false;
        options.IncludeLinks = true;
        options.MaxDepth = 5;
        options.PageNumberFormat = "Page {0}";
        options.CustomTitle = "Test Document";
        options.Encoding = Encoding.Unicode;

        // Assert
        options.IndentString.Should().Be("\t");
        options.IncludePageNumbers.Should().BeFalse();
        options.IncludeLinks.Should().BeTrue();
        options.MaxDepth.Should().Be(5);
        options.PageNumberFormat.Should().Be("Page {0}");
        options.CustomTitle.Should().Be("Test Document");
        options.Encoding.Should().Be(Encoding.Unicode);
    }

    [Fact]
    public void DefaultValues_ShouldBeAppropriateForMostUseCases()
    {
        // Arrange & Act
        var options = new ExportOptions();

        // Assert
        // Two spaces is a common indentation
        options.IndentString.Should().Be("  ");
        
        // Page numbers are usually wanted
        options.IncludePageNumbers.Should().BeTrue();
        
        // Links are not always supported by all formats
        options.IncludeLinks.Should().BeFalse();
        
        // No depth limit by default
        options.MaxDepth.Should().Be(0);
        
        // Chinese format is appropriate for the library's target audience
        options.PageNumberFormat.Should().Be("第 {0} 页");
        
        // UTF-8 is the most common encoding
        options.Encoding.Should().Be(Encoding.UTF8);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("    ")]
    public void IndentString_ShouldAcceptVariousWhitespaceValues(string indent)
    {
        // Arrange
        var options = new ExportOptions();

        // Act
        options.IndentString = indent;

        // Assert
        options.IndentString.Should().Be(indent);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void MaxDepth_ShouldAcceptNegativeValues(int depth)
    {
        // Arrange
        var options = new ExportOptions();

        // Act
        options.MaxDepth = depth;

        // Assert
        options.MaxDepth.Should().Be(depth);
    }
}
