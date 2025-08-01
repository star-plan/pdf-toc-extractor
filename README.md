# PdfTocExtractor

一个用于从PDF文件提取目录（TOC）的.NET库，支持多种输出格式。

## 功能特性

- 🔍 从PDF文件提取书签/目录信息
- 📄 支持多种输出格式：Markdown、JSON、XML、纯文本
- 🎯 可配置的导出选项（层级深度、页码格式等）
- 🔧 可扩展的导出器架构
- ⚡ 异步操作支持
- 🛠️ 命令行工具和NuGet包

## 安装

### NuGet包
```bash
dotnet add package PdfTocExtractor
```

### 命令行工具
```bash
dotnet tool install -g PdfTocExtractor.Cli
```

## 使用方法

### 作为库使用

```csharp
using PdfTocExtractor;
using PdfTocExtractor.Exporters;

// 创建提取器实例
var extractor = new PdfTocExtractor();

// 提取目录
var tocItems = await extractor.ExtractTocAsync("document.pdf");

// 导出为Markdown
await extractor.ExportToFileAsync(tocItems, "output.md", "markdown");

// 导出为JSON
var exportOptions = new ExportOptions
{
    MaxDepth = 3,
    IncludePageNumbers = true,
    CustomTitle = "文档目录"
};
await extractor.ExportToFileAsync(tocItems, "output.json", "json", exportOptions);

// 直接从PDF提取并导出
await extractor.ExtractAndExportAsync("document.pdf", "output.xml");
```

### 命令行工具使用

```bash
# 基本用法
pdftoc extract document.pdf -o output.md

# 指定格式
pdftoc extract document.pdf -o output.json -f json

# 设置最大层级深度
pdftoc extract document.pdf -o output.xml --max-depth 3

# 自定义标题和格式
pdftoc extract document.pdf -o output.txt --title "我的文档目录" --page-format "页码: {0}"

# 显示详细输出
pdftoc extract document.pdf -o output.md -v
```

## 支持的输出格式

- **Markdown** (`md`, `markdown`) - 适合文档和网页显示
- **JSON** (`json`) - 适合程序处理和API
- **XML** (`xml`) - 结构化数据交换
- **Text** (`txt`, `text`) - 纯文本格式

## 导出选项

- `MaxDepth` - 最大层级深度（0表示无限制）
- `IncludePageNumbers` - 是否包含页码信息
- `IncludeLinks` - 是否包含链接（如果格式支持）
- `CustomTitle` - 自定义文档标题
- `IndentString` - 缩进字符串
- `PageNumberFormat` - 页码格式化字符串
- `Encoding` - 文件编码格式

## 扩展性

您可以通过实现 `IExporter` 接口来创建自定义导出器：

```csharp
public class CustomExporter : IExporter
{
    public string FormatName => "Custom";
    public string FileExtension => "custom";

    public string Export(IEnumerable<TocItem> tocItems, ExportOptions? options = null)
    {
        // 实现自定义导出逻辑
        return "custom format content";
    }

    public async Task ExportToFileAsync(IEnumerable<TocItem> tocItems, string filePath, ExportOptions? options = null)
    {
        var content = Export(tocItems, options);
        await File.WriteAllTextAsync(filePath, content);
    }
}

// 注册自定义导出器
extractor.RegisterExporter("custom", new CustomExporter());
```

## 系统要求

- .NET 8.0 或更高版本
- 支持Windows、Linux、macOS

## 依赖项

- iText 9.2.0 - PDF处理（现代化的PDF库，替代了iTextSharp）
- Newtonsoft.Json 13.0.3 - JSON序列化
- System.CommandLine 2.0.0-beta4 - 命令行解析（仅CLI工具）

## 许可证

MIT License

## 贡献

欢迎提交Issue和Pull Request！
