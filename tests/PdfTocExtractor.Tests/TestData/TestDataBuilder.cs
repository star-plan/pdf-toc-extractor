using PdfTocExtractor.Models;

namespace PdfTocExtractor.Tests.TestData;

/// <summary>
/// 用于构建测试数据的辅助类
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// 创建简单的单级目录项
    /// </summary>
    public static List<TocItem> CreateSimpleTocItems()
    {
        return new List<TocItem>
        {
            new TocItem { Title = "Chapter 1", Page = "1", Level = 0 },
            new TocItem { Title = "Chapter 2", Page = "10", Level = 0 },
            new TocItem { Title = "Chapter 3", Page = "20", Level = 0 }
        };
    }

    /// <summary>
    /// 创建带有层级结构的目录项
    /// </summary>
    public static List<TocItem> CreateHierarchicalTocItems()
    {
        var chapter1 = new TocItem { Title = "Chapter 1: Introduction", Page = "1", Level = 0 };
        var section11 = new TocItem { Title = "1.1 Overview", Page = "2", Level = 1, Parent = chapter1 };
        var section12 = new TocItem { Title = "1.2 Objectives", Page = "5", Level = 1, Parent = chapter1 };
        var subsection121 = new TocItem { Title = "1.2.1 Primary Goals", Page = "6", Level = 2, Parent = section12 };
        var subsection122 = new TocItem { Title = "1.2.2 Secondary Goals", Page = "8", Level = 2, Parent = section12 };

        chapter1.Children.AddRange(new[] { section11, section12 });
        section12.Children.AddRange(new[] { subsection121, subsection122 });

        var chapter2 = new TocItem { Title = "Chapter 2: Methodology", Page = "15", Level = 0 };
        var section21 = new TocItem { Title = "2.1 Research Design", Page = "16", Level = 1, Parent = chapter2 };
        var section22 = new TocItem { Title = "2.2 Data Collection", Page = "20", Level = 1, Parent = chapter2 };

        chapter2.Children.AddRange(new[] { section21, section22 });

        var chapter3 = new TocItem { Title = "Chapter 3: Results", Page = "30", Level = 0 };

        return new List<TocItem> { chapter1, chapter2, chapter3 };
    }

    /// <summary>
    /// 创建深层嵌套的目录项（用于测试最大深度限制）
    /// </summary>
    public static List<TocItem> CreateDeepNestedTocItems()
    {
        var level0 = new TocItem { Title = "Level 0", Page = "1", Level = 0 };
        var level1 = new TocItem { Title = "Level 1", Page = "2", Level = 1, Parent = level0 };
        var level2 = new TocItem { Title = "Level 2", Page = "3", Level = 2, Parent = level1 };
        var level3 = new TocItem { Title = "Level 3", Page = "4", Level = 3, Parent = level2 };
        var level4 = new TocItem { Title = "Level 4", Page = "5", Level = 4, Parent = level3 };

        level0.Children.Add(level1);
        level1.Children.Add(level2);
        level2.Children.Add(level3);
        level3.Children.Add(level4);

        return new List<TocItem> { level0 };
    }

    /// <summary>
    /// 创建包含特殊字符的目录项
    /// </summary>
    public static List<TocItem> CreateSpecialCharacterTocItems()
    {
        return new List<TocItem>
        {
            new TocItem { Title = "Chapter <1> & \"Introduction\"", Page = "1", Level = 0 },
            new TocItem { Title = "Section with 'quotes' and symbols: @#$%", Page = "5", Level = 0 },
            new TocItem { Title = "Unicode: 测试章节 🔍 📄", Page = "10", Level = 0 },
            new TocItem { Title = "XML entities: &lt;&gt;&amp;", Page = "15", Level = 0 }
        };
    }

    /// <summary>
    /// 创建包含各种页码格式的目录项
    /// </summary>
    public static List<TocItem> CreateVariousPageFormatTocItems()
    {
        return new List<TocItem>
        {
            new TocItem { Title = "Normal Page", Page = "5", Level = 0 },
            new TocItem { Title = "Complex Page", Page = "10 XYZ 123 456", Level = 0 },
            new TocItem { Title = "Empty Page", Page = "", Level = 0 },
            new TocItem { Title = "No Page Info", Page = "无页码", Level = 0 },
            new TocItem { Title = "N/A Page", Page = "N/A", Level = 0 },
            new TocItem { Title = "Invalid Page", Page = "abc", Level = 0 }
        };
    }

    /// <summary>
    /// 创建空的目录项列表
    /// </summary>
    public static List<TocItem> CreateEmptyTocItems()
    {
        return new List<TocItem>();
    }

    /// <summary>
    /// 创建大量目录项（用于性能测试）
    /// </summary>
    public static List<TocItem> CreateLargeTocItems(int count = 100)
    {
        var items = new List<TocItem>();
        
        for (int i = 1; i <= count; i++)
        {
            var chapter = new TocItem 
            { 
                Title = $"Chapter {i}", 
                Page = (i * 10).ToString(), 
                Level = 0 
            };

            // 每5个章节添加一些子节
            if (i % 5 == 0)
            {
                for (int j = 1; j <= 3; j++)
                {
                    var section = new TocItem
                    {
                        Title = $"{i}.{j} Section {j}",
                        Page = (i * 10 + j).ToString(),
                        Level = 1,
                        Parent = chapter
                    };
                    chapter.Children.Add(section);
                }
            }

            items.Add(chapter);
        }

        return items;
    }

    /// <summary>
    /// 创建模拟PDF书签数据（用于模拟iTextSharp的书签格式）
    /// </summary>
    public static List<Dictionary<string, object>> CreateMockBookmarkData()
    {
        return new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                ["Title"] = "Chapter 1",
                ["Page"] = "1 XYZ 0 792 0",
                ["Kids"] = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        ["Title"] = "Section 1.1",
                        ["Page"] = "2 XYZ 0 792 0"
                    },
                    new Dictionary<string, object>
                    {
                        ["Title"] = "Section 1.2",
                        ["Page"] = "5 XYZ 0 792 0"
                    }
                }
            },
            new Dictionary<string, object>
            {
                ["Title"] = "Chapter 2",
                ["Page"] = "10 XYZ 0 792 0"
            }
        };
    }

    /// <summary>
    /// 创建包含无效书签数据的模拟数据
    /// </summary>
    public static List<Dictionary<string, object>> CreateInvalidMockBookmarkData()
    {
        return new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                // 缺少Title
                ["Page"] = "1 XYZ 0 792 0"
            },
            new Dictionary<string, object>
            {
                ["Title"] = "Valid Chapter",
                // 缺少Page
            },
            new Dictionary<string, object>
            {
                ["Title"] = "Chapter with Invalid Page",
                ["Page"] = "invalid page format"
            }
        };
    }

    /// <summary>
    /// 创建复杂的嵌套书签数据
    /// </summary>
    public static List<Dictionary<string, object>> CreateComplexMockBookmarkData()
    {
        return new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                ["Title"] = "Part I: Introduction",
                ["Page"] = "1 XYZ 0 792 0",
                ["Kids"] = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        ["Title"] = "Chapter 1: Overview",
                        ["Page"] = "3 XYZ 0 792 0",
                        ["Kids"] = new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object>
                            {
                                ["Title"] = "1.1 Background",
                                ["Page"] = "4 XYZ 0 792 0"
                            },
                            new Dictionary<string, object>
                            {
                                ["Title"] = "1.2 Scope",
                                ["Page"] = "7 XYZ 0 792 0",
                                ["Kids"] = new List<Dictionary<string, object>>
                                {
                                    new Dictionary<string, object>
                                    {
                                        ["Title"] = "1.2.1 Technical Scope",
                                        ["Page"] = "8 XYZ 0 792 0"
                                    },
                                    new Dictionary<string, object>
                                    {
                                        ["Title"] = "1.2.2 Business Scope",
                                        ["Page"] = "10 XYZ 0 792 0"
                                    }
                                }
                            }
                        }
                    },
                    new Dictionary<string, object>
                    {
                        ["Title"] = "Chapter 2: Methodology",
                        ["Page"] = "15 XYZ 0 792 0"
                    }
                }
            },
            new Dictionary<string, object>
            {
                ["Title"] = "Part II: Implementation",
                ["Page"] = "25 XYZ 0 792 0"
            }
        };
    }
}
