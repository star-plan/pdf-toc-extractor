using System.Text.RegularExpressions;

namespace PdfTocExtractor.Semantic;

/// <summary>
/// 基于语义分析的标题识别器
/// </summary>
public class SemanticHeadingAnalyzer
{
    private readonly SemanticAnalysisOptions _options;

    // 操作动词词典
    private readonly HashSet<string> _actionWords = new()
    {
        "点击", "选择", "输入", "打开", "关闭", "设置", "配置", "进入", "退出", 
        "保存", "删除", "修改", "查看", "操作", "执行", "运行", "启动", "停止",
        "添加", "移除", "编辑", "更新", "刷新", "重置", "清除", "导入", "导出",
        "上传", "下载", "发送", "接收", "连接", "断开", "登录", "登出", "注册",
        "提交", "取消", "确认", "拒绝", "同意", "申请", "审批", "通过", "驳回"
    };

    // 标题关键词词典
    private readonly HashSet<string> _headingKeywords = new()
    {
        "管理", "系统", "功能", "概述", "介绍", "说明", "中心", "平台", "工具", 
        "环境", "配置", "设置", "模块", "组件", "服务", "接口", "协议", "标准",
        "规范", "流程", "方案", "策略", "政策", "制度", "规则", "原则", "方法",
        "技术", "架构", "框架", "结构", "设计", "开发", "部署", "运维", "监控",
        "安全", "权限", "认证", "授权", "加密", "解密", "备份", "恢复", "容灾",
        "性能", "优化", "调优", "测试", "验证", "评估", "分析", "统计", "报告"
    };

    // 章节指示词
    private readonly HashSet<string> _chapterIndicators = new()
    {
        "章", "节", "部分", "篇", "卷", "册", "编", "辑", "集", "段", "条", "款", "项"
    };

    public SemanticHeadingAnalyzer(SemanticAnalysisOptions? options = null)
    {
        _options = options ?? SemanticAnalysisOptions.Default;
    }

    /// <summary>
    /// 分析文本片段列表，识别标题
    /// </summary>
    public List<TextFragment> AnalyzeHeadings(List<TextFragment> fragments)
    {
        if (!fragments.Any())
            return new List<TextFragment>();

        // 1. 计算统计信息
        var stats = CalculateStatistics(fragments);
        
        // 2. 对每个片段进行语义分析
        foreach (var fragment in fragments)
        {
            fragment.SemanticResult = AnalyzeFragment(fragment, stats);
        }

        // 3. 筛选出可能的标题
        var potentialHeadings = fragments
            .Where(f => f.SemanticResult?.IsLikelyHeading == true)
            .OrderBy(f => f.PageNumber)
            .ThenBy(f => f.Y)
            .ToList();

        // 4. 确定标题层级
        DetermineHeadingLevels(potentialHeadings);

        if (_options.DebugMode)
        {
            Console.WriteLine($"语义分析完成: 从 {fragments.Count} 个片段中识别出 {potentialHeadings.Count} 个标题");
            foreach (var heading in potentialHeadings.Take(10))
            {
                Console.WriteLine($"  [{heading.SemanticResult?.EstimatedLevel}] {heading.Text} (置信度: {heading.SemanticResult?.HeadingConfidence:F2})");
            }
        }

        return potentialHeadings;
    }

    /// <summary>
    /// 分析单个文本片段
    /// </summary>
    private SemanticAnalysisResult AnalyzeFragment(TextFragment fragment, TextStatistics stats)
    {
        var result = new SemanticAnalysisResult();
        var reasons = new List<string>();
        var exclusions = new List<string>();

        var text = fragment.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            exclusions.Add("空文本");
            result.ExclusionReasons = exclusions;
            return result;
        }

        // 1. 长度检查
        if (text.Length < _options.MinHeadingLength)
        {
            exclusions.Add($"文本太短 ({text.Length} < {_options.MinHeadingLength})");
        }
        else if (text.Length > _options.MaxHeadingLength)
        {
            exclusions.Add($"文本太长 ({text.Length} > {_options.MaxHeadingLength})");
        }
        else
        {
            reasons.Add("长度适中");
        }

        // 2. 操作动词检查
        if (ContainsActionWords(text))
        {
            exclusions.Add("包含操作动词");
        }
        else
        {
            reasons.Add("不包含操作动词");
        }

        // 3. 标题关键词检查
        if (ContainsHeadingKeywords(text))
        {
            reasons.Add("包含标题关键词");
        }

        // 4. 章节编号检查
        if (HasChapterNumbering(text))
        {
            reasons.Add("包含章节编号");
        }

        // 5. 字体特征检查
        if (fragment.FontSize > stats.AverageFontSize * _options.FontSizeMultiplier)
        {
            reasons.Add($"字体较大 ({fragment.FontSize:F1} > {stats.AverageFontSize * _options.FontSizeMultiplier:F1})");
        }

        if (fragment.IsBold && _options.ConsiderBoldAsHeading)
        {
            reasons.Add("粗体文本");
        }

        // 6. 位置特征检查
        if (fragment.IsStandalone)
        {
            reasons.Add("独立成行");
        }

        if (fragment.VerticalSpaceBefore > _options.MinVerticalSpacing || 
            fragment.VerticalSpaceAfter > _options.MinVerticalSpacing)
        {
            reasons.Add("前后有足够间距");
        }

        // 7. 排除明显的非标题内容
        if (IsObviouslyNotHeading(text))
        {
            exclusions.Add("明显的非标题内容");
        }

        // 8. 计算置信度
        float confidence = CalculateConfidence(reasons, exclusions, fragment, stats);

        result.IsLikelyHeading = confidence >= _options.MinConfidenceThreshold && !exclusions.Any();
        result.HeadingConfidence = confidence;
        result.Reasons = reasons;
        result.ExclusionReasons = exclusions;

        return result;
    }

    /// <summary>
    /// 检查是否包含操作动词
    /// </summary>
    private bool ContainsActionWords(string text)
    {
        return _actionWords.Any(word => text.Contains(word));
    }

    /// <summary>
    /// 检查是否包含标题关键词
    /// </summary>
    private bool ContainsHeadingKeywords(string text)
    {
        return _headingKeywords.Any(word => text.Contains(word));
    }

    /// <summary>
    /// 检查是否有章节编号
    /// </summary>
    private bool HasChapterNumbering(string text)
    {
        var patterns = new[]
        {
            @"^第[一二三四五六七八九十\d]+[章节部分篇]",  // 第一章、第二节
            @"^\d+(\.\d+)*[\.、]",                    // 1.2.3. 或 1.2.3、
            @"^[一二三四五六七八九十]+[、．.]",          // 一、二、
            @"^\([一二三四五六七八九十\d]+\)",          // (1) (一)
            @"^[A-Z]\.",                             // A. B.
        };

        return patterns.Any(pattern => Regex.IsMatch(text, pattern));
    }

    /// <summary>
    /// 检查是否明显不是标题
    /// </summary>
    private bool IsObviouslyNotHeading(string text)
    {
        var excludePatterns = new[]
        {
            @"^\d+$",                    // 纯数字
            @"^第\s*\d+\s*页$",          // 页码
            @"^Page\s+\d+$",             // 英文页码
            @"[\.]{5,}",                 // 多个点（目录页特征）
            @"\d+\.\d+\.\d+\.\d+",       // IP地址
            @":\d+",                     // 端口号
            @"^www\.|^http|@",           // 网址邮箱
            @"^\d{4}[-/]\d{1,2}[-/]\d{1,2}", // 日期
            @"^[\d\s\-\+\(\)]{10,}$",    // 长数字串
        };

        return excludePatterns.Any(pattern => Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// 计算置信度
    /// </summary>
    private float CalculateConfidence(List<string> reasons, List<string> exclusions, 
        TextFragment fragment, TextStatistics stats)
    {
        if (exclusions.Any())
            return 0f;

        float confidence = 0f;

        // 基础分数
        confidence += 0.1f;

        // 各种特征的权重
        foreach (var reason in reasons)
        {
            confidence += reason switch
            {
                var r when r.Contains("章节编号") => 0.4f,
                var r when r.Contains("标题关键词") => 0.3f,
                var r when r.Contains("字体较大") => 0.2f,
                var r when r.Contains("粗体文本") => 0.15f,
                var r when r.Contains("独立成行") => 0.1f,
                var r when r.Contains("足够间距") => 0.1f,
                var r when r.Contains("不包含操作动词") => 0.05f,
                _ => 0.02f
            };
        }

        return Math.Min(1.0f, confidence);
    }

    /// <summary>
    /// 确定标题层级
    /// </summary>
    private void DetermineHeadingLevels(List<TextFragment> headings)
    {
        if (!headings.Any())
            return;

        foreach (var heading in headings)
        {
            // 首先尝试从编号中获取层级
            var level = GetLevelFromNumbering(heading.Text);
            if (level > 0)
            {
                heading.SemanticResult!.EstimatedLevel = level;
                continue;
            }

            // 否则基于字体大小确定层级
            var fontSizeRank = headings
                .Select(h => h.FontSize)
                .Distinct()
                .OrderByDescending(s => s)
                .ToList()
                .IndexOf(heading.FontSize) + 1;

            heading.SemanticResult!.EstimatedLevel = Math.Min(fontSizeRank, _options.MaxHeadingLevels);
        }
    }

    /// <summary>
    /// 从编号中获取层级
    /// </summary>
    private int GetLevelFromNumbering(string text)
    {
        // 匹配 1.2.3. 格式
        var match = Regex.Match(text, @"^(\d+(?:\.\d+)*)\.");
        if (match.Success)
        {
            return match.Groups[1].Value.Split('.').Length;
        }

        // 匹配第X章格式
        if (Regex.IsMatch(text, @"^第\s*[一二三四五六七八九十\d]+\s*章"))
            return 1;

        if (Regex.IsMatch(text, @"^第\s*[一二三四五六七八九十\d]+\s*节"))
            return 2;

        return 0;
    }

    /// <summary>
    /// 计算文本统计信息
    /// </summary>
    private TextStatistics CalculateStatistics(List<TextFragment> fragments)
    {
        var validFragments = fragments.Where(f => !string.IsNullOrWhiteSpace(f.Text)).ToList();
        
        if (!validFragments.Any())
            return new TextStatistics();

        return new TextStatistics
        {
            TotalFragments = validFragments.Count,
            AverageFontSize = validFragments.Average(f => f.FontSize),
            MaxFontSize = validFragments.Max(f => f.FontSize),
            MinFontSize = validFragments.Min(f => f.FontSize),
            BoldTextCount = validFragments.Count(f => f.IsBold),
            AverageTextLength = validFragments.Average(f => f.Text.Length)
        };
    }

    /// <summary>
    /// 文本统计信息
    /// </summary>
    private class TextStatistics
    {
        public int TotalFragments { get; set; }
        public float AverageFontSize { get; set; }
        public float MaxFontSize { get; set; }
        public float MinFontSize { get; set; }
        public int BoldTextCount { get; set; }
        public double AverageTextLength { get; set; }
    }
}
