namespace PdfTocExtractor.Semantic;

/// <summary>
/// 语义分析配置选项
/// </summary>
public class SemanticAnalysisOptions
{
    /// <summary>
    /// 标题的最小长度
    /// </summary>
    public int MinHeadingLength { get; set; } = 3;

    /// <summary>
    /// 标题的最大长度
    /// </summary>
    public int MaxHeadingLength { get; set; } = 100;

    /// <summary>
    /// 字体大小倍数阈值（相对于平均字体大小）
    /// </summary>
    public float FontSizeMultiplier { get; set; } = 1.1f;

    /// <summary>
    /// 是否将粗体视为标题指示器
    /// </summary>
    public bool ConsiderBoldAsHeading { get; set; } = true;

    /// <summary>
    /// 最小垂直间距
    /// </summary>
    public float MinVerticalSpacing { get; set; } = 5f;

    /// <summary>
    /// 最小置信度阈值
    /// </summary>
    public float MinConfidenceThreshold { get; set; } = 0.3f;

    /// <summary>
    /// 最大标题层级数
    /// </summary>
    public int MaxHeadingLevels { get; set; } = 6;

    /// <summary>
    /// 是否启用调试模式
    /// </summary>
    public bool DebugMode { get; set; } = false;

    /// <summary>
    /// 跳过的页面范围（通常是目录页）
    /// </summary>
    public List<int> SkipPages { get; set; } = new() { 1, 2, 3 };

    /// <summary>
    /// 是否忽略页眉页脚
    /// </summary>
    public bool IgnoreHeaderFooter { get; set; } = true;

    /// <summary>
    /// 页眉高度
    /// </summary>
    public float HeaderHeight { get; set; } = 50f;

    /// <summary>
    /// 页脚高度
    /// </summary>
    public float FooterHeight { get; set; } = 50f;

    /// <summary>
    /// 默认配置
    /// </summary>
    public static SemanticAnalysisOptions Default => new();

    /// <summary>
    /// 严格模式配置
    /// </summary>
    public static SemanticAnalysisOptions Strict => new()
    {
        MinHeadingLength = 5,
        MaxHeadingLength = 80,
        FontSizeMultiplier = 1.3f,
        MinConfidenceThreshold = 0.5f,
        MinVerticalSpacing = 8f
    };

    /// <summary>
    /// 宽松模式配置
    /// </summary>
    public static SemanticAnalysisOptions Relaxed => new()
    {
        MinHeadingLength = 2,
        MaxHeadingLength = 150,
        FontSizeMultiplier = 1.05f,
        MinConfidenceThreshold = 0.2f,
        MinVerticalSpacing = 2f
    };

    /// <summary>
    /// 调试模式配置
    /// </summary>
    public static SemanticAnalysisOptions Debug => new()
    {
        DebugMode = true,
        MinConfidenceThreshold = 0.1f
    };
}
