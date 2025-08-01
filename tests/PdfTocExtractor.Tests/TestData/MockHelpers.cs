using Moq;
using PdfTocExtractor.Exporters;
using PdfTocExtractor.Models;

namespace PdfTocExtractor.Tests.TestData;

/// <summary>
/// 用于创建Mock对象的辅助类
/// </summary>
public static class MockHelpers
{
    /// <summary>
    /// 创建Mock的IExporter
    /// </summary>
    public static Mock<IExporter> CreateMockExporter(string formatName = "Mock", string fileExtension = "mock")
    {
        var mock = new Mock<IExporter>();
        
        mock.Setup(x => x.FormatName).Returns(formatName);
        mock.Setup(x => x.FileExtension).Returns(fileExtension);
        
        mock.Setup(x => x.Export(It.IsAny<IEnumerable<TocItem>>(), It.IsAny<ExportOptions>()))
            .Returns((IEnumerable<TocItem> items, ExportOptions? options) => 
            {
                var title = options?.CustomTitle ?? "Mock Export";
                var itemCount = items.Count();
                return $"{title}\nItems: {itemCount}";
            });

        mock.Setup(x => x.ExportToFileAsync(
                It.IsAny<IEnumerable<TocItem>>(), 
                It.IsAny<string>(), 
                It.IsAny<ExportOptions>()))
            .Returns((IEnumerable<TocItem> items, string filePath, ExportOptions? options) =>
            {
                var content = mock.Object.Export(items, options);
                return File.WriteAllTextAsync(filePath, content);
            });

        return mock;
    }

    /// <summary>
    /// 创建抛出异常的Mock IExporter
    /// </summary>
    public static Mock<IExporter> CreateFailingMockExporter(Exception exception)
    {
        var mock = new Mock<IExporter>();
        
        mock.Setup(x => x.FormatName).Returns("FailingMock");
        mock.Setup(x => x.FileExtension).Returns("fail");
        
        mock.Setup(x => x.Export(It.IsAny<IEnumerable<TocItem>>(), It.IsAny<ExportOptions>()))
            .Throws(exception);

        mock.Setup(x => x.ExportToFileAsync(
                It.IsAny<IEnumerable<TocItem>>(), 
                It.IsAny<string>(), 
                It.IsAny<ExportOptions>()))
            .ThrowsAsync(exception);

        return mock;
    }

    /// <summary>
    /// 创建验证调用的Mock IExporter
    /// </summary>
    public static Mock<IExporter> CreateVerifiableMockExporter()
    {
        var mock = new Mock<IExporter>();
        
        mock.Setup(x => x.FormatName).Returns("Verifiable");
        mock.Setup(x => x.FileExtension).Returns("verify");
        
        mock.Setup(x => x.Export(It.IsAny<IEnumerable<TocItem>>(), It.IsAny<ExportOptions>()))
            .Returns("Verified Export")
            .Verifiable();

        mock.Setup(x => x.ExportToFileAsync(
                It.IsAny<IEnumerable<TocItem>>(), 
                It.IsAny<string>(), 
                It.IsAny<ExportOptions>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        return mock;
    }

    /// <summary>
    /// 创建模拟文件系统操作的临时文件
    /// </summary>
    public static string CreateTempFile(string content = "")
    {
        var tempFile = Path.GetTempFileName();
        if (!string.IsNullOrEmpty(content))
        {
            File.WriteAllText(tempFile, content);
        }
        return tempFile;
    }

    /// <summary>
    /// 创建临时目录
    /// </summary>
    public static string CreateTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }

    /// <summary>
    /// 清理临时文件
    /// </summary>
    public static void CleanupTempFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // 忽略清理错误
        }
    }

    /// <summary>
    /// 清理临时目录
    /// </summary>
    public static void CleanupTempDirectory(string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }
        catch
        {
            // 忽略清理错误
        }
    }

    /// <summary>
    /// 创建测试用的ExportOptions
    /// </summary>
    public static ExportOptions CreateTestExportOptions(
        string? customTitle = null,
        bool includePageNumbers = true,
        bool includeLinks = false,
        int maxDepth = 0,
        string indentString = "  ",
        string pageNumberFormat = "第 {0} 页")
    {
        return new ExportOptions
        {
            CustomTitle = customTitle,
            IncludePageNumbers = includePageNumbers,
            IncludeLinks = includeLinks,
            MaxDepth = maxDepth,
            IndentString = indentString,
            PageNumberFormat = pageNumberFormat
        };
    }

    /// <summary>
    /// 验证两个TocItem列表是否相等（用于测试）
    /// </summary>
    public static bool AreTocItemListsEqual(IEnumerable<TocItem> list1, IEnumerable<TocItem> list2)
    {
        var items1 = list1.ToList();
        var items2 = list2.ToList();

        if (items1.Count != items2.Count)
            return false;

        for (int i = 0; i < items1.Count; i++)
        {
            if (!AreTocItemsEqual(items1[i], items2[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 验证两个TocItem是否相等（递归比较）
    /// </summary>
    public static bool AreTocItemsEqual(TocItem item1, TocItem item2)
    {
        if (item1.Title != item2.Title ||
            item1.Page != item2.Page ||
            item1.Level != item2.Level ||
            item1.Children.Count != item2.Children.Count)
        {
            return false;
        }

        for (int i = 0; i < item1.Children.Count; i++)
        {
            if (!AreTocItemsEqual(item1.Children[i], item2.Children[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 创建测试用的PDF文件路径（不存在的文件）
    /// </summary>
    public static string CreateNonExistentPdfPath()
    {
        return Path.Combine(Path.GetTempPath(), $"non_existent_{Guid.NewGuid()}.pdf");
    }

    /// <summary>
    /// 创建测试用的空PDF文件
    /// </summary>
    public static string CreateEmptyPdfFile()
    {
        var tempFile = Path.GetTempFileName();
        var pdfPath = Path.ChangeExtension(tempFile, ".pdf");
        File.Move(tempFile, pdfPath);
        
        // 创建一个空的PDF文件（仅用于测试文件存在性）
        File.WriteAllBytes(pdfPath, new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF header
        
        return pdfPath;
    }

    /// <summary>
    /// 验证字符串是否为有效的JSON格式
    /// </summary>
    public static bool IsValidJson(string jsonString)
    {
        try
        {
            Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 验证字符串是否为有效的XML格式
    /// </summary>
    public static bool IsValidXml(string xmlString)
    {
        try
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xmlString);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
