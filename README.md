# PdfTocExtractor

![License](https://img.shields.io/badge/license-MIT-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Build Status](https://github.com/star-plan/pdf-toc-extractor/workflows/持续集成/badge.svg)
![NuGet](https://img.shields.io/nuget/v/PdfTocExtractor)

PdfTocExtractor 是一个纯 C# AOT 实现的轻量级PDF目录提取工具，用于从PDF文件中提取目录（TOC）并导出为多种格式。支持Markdown、JSON、XML、纯文本等格式，完全摆脱命令行依赖，无需额外的PDF处理工具，适合在 .NET 项目中内嵌、分发或集成自动化流程中使用。

通过PdfTocExtractor，您可以轻松从PDF文档中提取书签和目录结构，生成清晰的导航文档，完美适用于文档处理、内容分析和自动化工作流。

🚀 跨平台、零依赖、极速提取，一切尽在 PdfTocExtractor！

## ✨ 功能特点

- 📖 从PDF文件提取书签/目录信息
- 📄 支持多种输出格式：Markdown、JSON、XML、纯文本
- 🎯 可配置的导出选项（层级深度、页码格式等）
- 🔧 可扩展的导出器架构，支持自定义格式
- ⚡ 异步操作支持，高性能处理
- 🛠️ 提供命令行工具和NuGet包
- 🚀 支持AOT编译，原生性能无需.NET运行时
- 🌐 跨平台支持：Windows、Linux、macOS

## 📦 安装

### 作为 .NET Global Tool 安装

```bash
# 安装核心库
dotnet add package PdfTocExtractor

# 安装CLI工具
dotnet tool install --global PdfTocExtractor.Cli
```

### 从 GitHub Releases 下载

访问 [Releases页面](https://github.com/star-plan/pdf-toc-extractor/releases) 下载适合您平台的可执行文件：

- **Windows**: `PdfTocExtractor-windows-{version}.zip`
- **Linux**: `PdfTocExtractor-linux-{version}.tar.gz`
- **macOS**: `PdfTocExtractor-macOS-{version}.tar.gz`

### 从源码构建

```bash
git clone https://github.com/star-plan/pdf-toc-extractor.git
cd PdfTocExtractor
dotnet build -c Release
```

## 🚀 使用方法

### 命令行工具使用

```bash
# 基本用法 - 提取PDF目录并保存为Markdown
pdftoc extract document.pdf -o output.md

# 指定输出格式
pdftoc extract document.pdf -o output.json -f json

# 设置最大层级深度
pdftoc extract document.pdf -o output.xml --max-depth 3

# 自定义标题和页码格式
pdftoc extract document.pdf -o output.txt --title "我的文档目录" --page-format "第 {0} 页"

# 包含页码和链接信息
pdftoc extract document.pdf -o output.md --include-pages --include-links

# 显示详细输出
pdftoc extract document.pdf -o output.md --verbose

# 诊断PDF文件问题（当遇到读取错误时很有用）
pdftoc diagnose document.pdf
```

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

// 导出为JSON（带自定义选项）
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

## 📋 参数说明

### 提取命令(extract)

| 参数 | 缩写 | 说明 | 是否必需 |
| --- | --- | --- | --- |
| `input` | - | 输入PDF文件路径 | 是 |
| `--output` | `-o` | 输出文件路径 | 否，默认为控制台输出 |
| `--format` | `-f` | 输出格式 (markdown/json/xml/text) | 否，默认为markdown |
| `--max-depth` | - | 最大层级深度 | 否，默认为0（无限制） |
| `--include-pages` | - | 包含页码信息 | 否，默认为true |
| `--include-links` | - | 包含链接信息 | 否，默认为false |
| `--title` | - | 自定义文档标题 | 否 |
| `--indent` | - | 缩进字符串 | 否，默认为"  " |
| `--page-format` | - | 页码格式化字符串 | 否，默认为"第 {0} 页" |
| `--verbose` | `-v` | 显示详细输出 | 否 |

## 📄 支持的输出格式

- **Markdown** (`md`, `markdown`) - 适合文档和网页显示，支持层级结构
- **JSON** (`json`) - 适合程序处理和API集成，包含完整元数据
- **XML** (`xml`) - 结构化数据交换，标准化格式
- **Text** (`txt`, `text`) - 纯文本格式，简洁易读

## ⚙️ 导出选项

- `MaxDepth` - 最大层级深度（0表示无限制）
- `IncludePageNumbers` - 是否包含页码信息
- `IncludeLinks` - 是否包含链接（如果格式支持）
- `CustomTitle` - 自定义文档标题
- `IndentString` - 缩进字符串
- `PageNumberFormat` - 页码格式化字符串
- `Encoding` - 文件编码格式

## 📝 示例输出

### 提取PDF目录

```
正在从 document.pdf 提取目录...
找到 15 个目录项
正在导出为 Markdown 格式...
目录已成功导出到: output.md
```

### Markdown 输出示例

```markdown
# 文档目录

- [第1章 概述](#第1章-概述) (第 1 页)
  - [1.1 背景](#11-背景) (第 2 页)
  - [1.2 目标](#12-目标) (第 3 页)
- [第2章 技术架构](#第2章-技术架构) (第 5 页)
  - [2.1 系统设计](#21-系统设计) (第 6 页)
    - [2.1.1 核心组件](#211-核心组件) (第 7 页)
    - [2.1.2 数据流](#212-数据流) (第 8 页)
```

### JSON 输出示例

```json
{
  "title": "文档目录",
  "generatedAt": "2024-01-15T10:30:00",
  "items": [
    {
      "title": "第1章 概述",
      "level": 1,
      "page": "第 1 页",
      "pageNumber": 1,
      "children": [
        {
          "title": "1.1 背景",
          "level": 2,
          "page": "第 2 页",
          "pageNumber": 2
        }
      ]
    }
  ]
}
```

## 🔧 扩展性

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

## 🔧 故障排除

### 常见问题

#### "PdfEncryption exception" 错误

如果遇到此错误，通常是因为PDF文件使用了加密或权限保护。请尝试以下解决方案：

1. **使用诊断命令**：
   ```bash
   pdftoc diagnose your-document.pdf
   ```
   这会显示PDF文件的详细信息，帮助诊断问题。

2. **检查依赖**：确保已安装 `itext7.bouncy-castle-adapter` 包：
   ```bash
   dotnet add package itext7.bouncy-castle-adapter
   ```

3. **PDF文件类型**：
   - ✅ 支持：有权限保护但无用户密码的PDF
   - ✅ 支持：未加密的PDF
   - ❌ 不支持：需要用户密码的PDF（功能开发中）

#### "此PDF文件没有目录（书签）信息" 错误

这表示PDF文件确实没有嵌入的书签/目录信息。可以：
- 使用诊断命令确认：`pdftoc diagnose your-document.pdf`
- 检查PDF是否在其他阅读器中显示目录面板
- 考虑使用其他工具为PDF添加书签

#### 输出文件为空或格式错误

1. 检查输入PDF是否有有效的目录结构
2. 尝试不同的输出格式：`-f json` 或 `-f xml`
3. 使用 `--verbose` 选项查看详细处理信息

## 🛠️ 技术实现

PdfTocExtractor 使用以下技术：

- **.NET 8.0** - 现代化的.NET平台
- **[iText 9.2.0](https://github.com/itext/itext7-dotnet)** - 强大的PDF处理库
- **[iText7.bouncy-castle-adapter 9.2.0](https://www.nuget.org/packages/itext7.bouncy-castle-adapter)** - 加密PDF支持（必需）
- **[Newtonsoft.Json 13.0.3](https://github.com/JamesNK/Newtonsoft.Json)** - JSON序列化
- **[System.CommandLine 2.0.0](https://github.com/dotnet/command-line-api)** - 命令行参数解析
- **AOT编译支持** - 原生性能，无需.NET运行时

## 📄 许可证

MIT License

## 🚀 开发者指南

### 构建项目

```bash
# 克隆仓库
git clone https://github.com/star-plan/pdf-toc-extractor.git
cd PdfTocExtractor

# 恢复依赖
dotnet restore

# 构建项目
dotnet build -c Release

# 运行测试
dotnet test
```

### 发布模式

PdfTocExtractor 支持两种发布模式：

#### AOT 发布 (原生性能，无需 .NET 运行时)

```bash
# Windows
dotnet publish src/PdfTocExtractor.Cli/PdfTocExtractor.Cli.csproj -c Release -r win-x64 --self-contained true -p:PublishAot=true

# Linux
dotnet publish src/PdfTocExtractor.Cli/PdfTocExtractor.Cli.csproj -c Release -r linux-x64 --self-contained true -p:PublishAot=true

# macOS
dotnet publish src/PdfTocExtractor.Cli/PdfTocExtractor.Cli.csproj -c Release -r osx-x64 --self-contained true -p:PublishAot=true
```

#### Framework Dependent 发布 (需要 .NET 运行时)

```bash
# 发布为 NuGet 包 (.NET Tool)
dotnet pack

# 安装本地打包的工具
dotnet tool install --global --add-source ./src/PdfTocExtractor.Cli/nupkg PdfTocExtractor.Cli
```

### 本地测试

#### 测试 .NET Tool

```bash
# 安装本地打包的工具
dotnet tool install --global --add-source ./src/PdfTocExtractor.Cli/nupkg PdfTocExtractor.Cli

# 测试工具
pdftoc extract sample.pdf -o test-output.md

# 卸载工具
dotnet tool uninstall --global PdfTocExtractor.Cli
```

#### 测试 AOT 发布版本

```bash
# 直接运行生成的可执行文件
./publish/pdftoc extract sample.pdf -o test-output.md
```

## 🤝 贡献

欢迎提交Issue和Pull Request！

### 贡献指南

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## � TODO 计划

### 🤖 AI 增强功能
- [ ] **AI 目录识别** - 使用机器学习技术识别没有书签/大纲数据的PDF文档目录结构
  - [ ] 集成 OCR 技术识别扫描版PDF中的目录页面
  - [ ] 基于文本分析和格式识别的智能目录提取
  - [ ] 支持多语言目录识别（中文、英文、日文等）
  - [ ] 目录层级结构智能推断
  - [ ] 页码与目录项的自动匹配算法

### 🌐 Web 服务接口
- [ ] **RESTful API 开发** - 提供基于 HTTP 的 Web API 服务
  - [ ] PDF 文件上传接口
  - [ ] 目录提取 API 端点
  - [ ] 多格式导出 API（JSON、XML、Markdown等）
  - [ ] 批量处理接口
  - [ ] 任务状态查询接口
  - [ ] API 文档和 Swagger 集成

### 🖥️ Web 前端界面
- [ ] **现代化 Web UI** - 开发用户友好的 Web 前端界面
  - [ ] 拖拽式 PDF 文件上传
  - [ ] 实时目录提取进度显示
  - [ ] 目录结构可视化预览
  - [ ] 多格式导出选项界面
  - [ ] 历史记录和文件管理
  - [ ] 响应式设计，支持移动端访问

### 🔧 技术架构优化
- [ ] **微服务架构** - 将功能模块化为独立的微服务
  - [ ] PDF 处理服务
  - [ ] AI 识别服务
  - [ ] 文件存储服务
  - [ ] 用户管理服务
  - [ ] 容器化部署支持（Docker）

### 📊 高级功能
- [ ] **智能分析** - 提供更多文档分析功能
  - [ ] 文档结构分析和统计
  - [ ] 目录质量评估
  - [ ] 重复内容检测
  - [ ] 文档相似度比较
  - [ ] 批量文档处理和分析报告

### 🔐 企业级功能
- [ ] **安全和权限** - 企业级安全特性
  - [ ] 用户认证和授权
  - [ ] 文件访问权限控制
  - [ ] 审计日志记录
  - [ ] 数据加密存储
  - [ ] API 访问限制和配额管理

## �📞 支持

如果您遇到问题或有建议，请：

- 📋 [提交Issue](https://github.com/star-plan/pdf-toc-extractor/issues)
- 💬 [参与讨论](https://github.com/star-plan/pdf-toc-extractor/discussions)
- ⭐ 如果这个项目对您有帮助，请给个Star！
