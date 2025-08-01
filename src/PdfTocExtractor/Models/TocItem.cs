namespace PdfTocExtractor.Models;

/// <summary>
/// 表示PDF目录中的一个项目
/// </summary>
public class TocItem
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 页码
    /// </summary>
    public string Page { get; set; } = string.Empty;

    /// <summary>
    /// 层级深度（从0开始）
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 子项目
    /// </summary>
    public List<TocItem> Children { get; set; } = new();

    /// <summary>
    /// 父项目（用于构建层级关系）
    /// </summary>
    public TocItem? Parent { get; set; }

    /// <summary>
    /// 获取页码的数字部分
    /// </summary>
    public int PageNumber
    {
        get
        {
            if (string.IsNullOrEmpty(Page) || Page == "无页码" || Page == "N/A")
                return 0;

            // 处理 "5 XYZ ..." 格式，只取页码部分
            var pageStr = Page.Contains(' ') ? Page.Split(' ')[0] : Page;
            return int.TryParse(pageStr, out var pageNum) ? pageNum : 0;
        }
    }

    /// <summary>
    /// 是否有子项目
    /// </summary>
    public bool HasChildren => Children.Count > 0;

    /// <summary>
    /// 获取所有后代项目（递归）
    /// </summary>
    public IEnumerable<TocItem> GetAllDescendants()
    {
        foreach (var child in Children)
        {
            yield return child;
            foreach (var descendant in child.GetAllDescendants())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// 获取项目的完整路径（从根到当前项目的标题路径）
    /// </summary>
    public string GetFullPath(string separator = " > ")
    {
        var path = new List<string>();
        var current = this;
        
        while (current != null)
        {
            path.Insert(0, current.Title);
            current = current.Parent;
        }
        
        return string.Join(separator, path);
    }

    public override string ToString()
    {
        return $"{new string(' ', Level * 2)}- {Title} (第 {Page} 页)";
    }
}
