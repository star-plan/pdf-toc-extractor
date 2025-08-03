namespace PdfTocExtractor.Semantic;

/// <summary>
/// 表示PDF中的一个文本片段及其上下文信息
/// </summary>
public class TextFragment
{
    /// <summary>
    /// 文本内容
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 字体大小
    /// </summary>
    public float FontSize { get; set; }

    /// <summary>
    /// 字体名称
    /// </summary>
    public string FontName { get; set; } = string.Empty;

    /// <summary>
    /// 是否为粗体
    /// </summary>
    public bool IsBold { get; set; }

    /// <summary>
    /// 是否为斜体
    /// </summary>
    public bool IsItalic { get; set; }

    /// <summary>
    /// X坐标位置
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Y坐标位置
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 文本宽度
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// 文本高度
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// 是否独立成行
    /// </summary>
    public bool IsStandalone { get; set; }

    /// <summary>
    /// 前后的垂直间距
    /// </summary>
    public float VerticalSpaceBefore { get; set; }
    public float VerticalSpaceAfter { get; set; }

    /// <summary>
    /// 语义分析结果
    /// </summary>
    public SemanticAnalysisResult? SemanticResult { get; set; }

    public override string ToString()
    {
        return $"[Page {PageNumber}] \"{Text}\" - Font: {FontName}, Size: {FontSize}, Bold: {IsBold}";
    }
}

/// <summary>
/// 语义分析结果
/// </summary>
public class SemanticAnalysisResult
{
    /// <summary>
    /// 是否可能是标题
    /// </summary>
    public bool IsLikelyHeading { get; set; }

    /// <summary>
    /// 标题置信度 (0-1)
    /// </summary>
    public float HeadingConfidence { get; set; }

    /// <summary>
    /// 推测的标题层级 (1-6)
    /// </summary>
    public int EstimatedLevel { get; set; }

    /// <summary>
    /// 分析原因
    /// </summary>
    public List<string> Reasons { get; set; } = new();

    /// <summary>
    /// 排除原因（如果不是标题）
    /// </summary>
    public List<string> ExclusionReasons { get; set; } = new();
}
