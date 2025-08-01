using FluentAssertions;
using PdfTocExtractor.Models;
using Xunit;

namespace PdfTocExtractor.Tests.Models;

public class TocItemTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var tocItem = new TocItem();

        // Assert
        tocItem.Title.Should().Be(string.Empty);
        tocItem.Page.Should().Be(string.Empty);
        tocItem.Level.Should().Be(0);
        tocItem.Children.Should().NotBeNull().And.BeEmpty();
        tocItem.Parent.Should().BeNull();
        tocItem.HasChildren.Should().BeFalse();
    }

    [Fact]
    public void Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var tocItem = new TocItem();
        var parent = new TocItem { Title = "Parent" };
        var child = new TocItem { Title = "Child" };

        // Act
        tocItem.Title = "Test Title";
        tocItem.Page = "5";
        tocItem.Level = 2;
        tocItem.Parent = parent;
        tocItem.Children.Add(child);

        // Assert
        tocItem.Title.Should().Be("Test Title");
        tocItem.Page.Should().Be("5");
        tocItem.Level.Should().Be(2);
        tocItem.Parent.Should().Be(parent);
        tocItem.Children.Should().HaveCount(1).And.Contain(child);
        tocItem.HasChildren.Should().BeTrue();
    }

    [Theory]
    [InlineData("5", 5)]
    [InlineData("10", 10)]
    [InlineData("1", 1)]
    [InlineData("100", 100)]
    public void PageNumber_ShouldReturnCorrectIntegerValue_WhenPageIsValidNumber(string page, int expected)
    {
        // Arrange
        var tocItem = new TocItem { Page = page };

        // Act
        var result = tocItem.PageNumber;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("5 XYZ 123 456", 5)]
    [InlineData("10 ABC DEF", 10)]
    [InlineData("1 Some Additional Info", 1)]
    public void PageNumber_ShouldReturnFirstNumber_WhenPageContainsSpaces(string page, int expected)
    {
        // Arrange
        var tocItem = new TocItem { Page = page };

        // Act
        var result = tocItem.PageNumber;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("无页码")]
    [InlineData("N/A")]
    [InlineData("abc")]
    [InlineData("invalid")]
    public void PageNumber_ShouldReturnZero_WhenPageIsInvalid(string page)
    {
        // Arrange
        var tocItem = new TocItem { Page = page };

        // Act
        var result = tocItem.PageNumber;

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void HasChildren_ShouldReturnTrue_WhenChildrenExist()
    {
        // Arrange
        var tocItem = new TocItem();
        var child = new TocItem { Title = "Child" };

        // Act
        tocItem.Children.Add(child);

        // Assert
        tocItem.HasChildren.Should().BeTrue();
    }

    [Fact]
    public void HasChildren_ShouldReturnFalse_WhenNoChildren()
    {
        // Arrange
        var tocItem = new TocItem();

        // Act & Assert
        tocItem.HasChildren.Should().BeFalse();
    }

    [Fact]
    public void GetAllDescendants_ShouldReturnEmptyCollection_WhenNoChildren()
    {
        // Arrange
        var tocItem = new TocItem { Title = "Root" };

        // Act
        var descendants = tocItem.GetAllDescendants().ToList();

        // Assert
        descendants.Should().BeEmpty();
    }

    [Fact]
    public void GetAllDescendants_ShouldReturnDirectChildren_WhenOnlyDirectChildren()
    {
        // Arrange
        var root = new TocItem { Title = "Root" };
        var child1 = new TocItem { Title = "Child1" };
        var child2 = new TocItem { Title = "Child2" };
        
        root.Children.Add(child1);
        root.Children.Add(child2);

        // Act
        var descendants = root.GetAllDescendants().ToList();

        // Assert
        descendants.Should().HaveCount(2);
        descendants.Should().Contain(child1);
        descendants.Should().Contain(child2);
    }

    [Fact]
    public void GetAllDescendants_ShouldReturnAllDescendants_WhenNestedChildren()
    {
        // Arrange
        var root = new TocItem { Title = "Root" };
        var child1 = new TocItem { Title = "Child1" };
        var child2 = new TocItem { Title = "Child2" };
        var grandchild1 = new TocItem { Title = "Grandchild1" };
        var grandchild2 = new TocItem { Title = "Grandchild2" };
        
        root.Children.Add(child1);
        root.Children.Add(child2);
        child1.Children.Add(grandchild1);
        child2.Children.Add(grandchild2);

        // Act
        var descendants = root.GetAllDescendants().ToList();

        // Assert
        descendants.Should().HaveCount(4);
        descendants.Should().Contain(child1);
        descendants.Should().Contain(child2);
        descendants.Should().Contain(grandchild1);
        descendants.Should().Contain(grandchild2);
    }

    [Fact]
    public void GetFullPath_ShouldReturnSingleTitle_WhenNoParent()
    {
        // Arrange
        var tocItem = new TocItem { Title = "Root" };

        // Act
        var path = tocItem.GetFullPath();

        // Assert
        path.Should().Be("Root");
    }

    [Fact]
    public void GetFullPath_ShouldReturnFullPath_WhenHasParents()
    {
        // Arrange
        var root = new TocItem { Title = "Root" };
        var child = new TocItem { Title = "Child", Parent = root };
        var grandchild = new TocItem { Title = "Grandchild", Parent = child };

        // Act
        var path = grandchild.GetFullPath();

        // Assert
        path.Should().Be("Root > Child > Grandchild");
    }

    [Fact]
    public void GetFullPath_ShouldUseCustomSeparator_WhenProvided()
    {
        // Arrange
        var root = new TocItem { Title = "Root" };
        var child = new TocItem { Title = "Child", Parent = root };
        var grandchild = new TocItem { Title = "Grandchild", Parent = child };

        // Act
        var path = grandchild.GetFullPath(" / ");

        // Assert
        path.Should().Be("Root / Child / Grandchild");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var tocItem = new TocItem 
        { 
            Title = "Test Title", 
            Page = "5", 
            Level = 2 
        };

        // Act
        var result = tocItem.ToString();

        // Assert
        result.Should().Be("    - Test Title (第 5 页)");
    }

    [Fact]
    public void ToString_ShouldIndentCorrectly_BasedOnLevel()
    {
        // Arrange
        var level0 = new TocItem { Title = "Level 0", Page = "1", Level = 0 };
        var level1 = new TocItem { Title = "Level 1", Page = "2", Level = 1 };
        var level2 = new TocItem { Title = "Level 2", Page = "3", Level = 2 };

        // Act & Assert
        level0.ToString().Should().Be("- Level 0 (第 1 页)");
        level1.ToString().Should().Be("  - Level 1 (第 2 页)");
        level2.ToString().Should().Be("    - Level 2 (第 3 页)");
    }
}
