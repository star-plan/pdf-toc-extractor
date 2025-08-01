# PdfTocExtractor.Cli

![License](https://img.shields.io/badge/license-MIT-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![NuGet](https://img.shields.io/nuget/v/PdfTocExtractor.Cli)

PdfTocExtractor.Cli 是一个纯 C# AOT 实现的轻量级PDF目录提取命令行工具，用于从PDF文件中提取目录（TOC）并导出为多种格式。支持Markdown、JSON、XML、纯文本等格式，完全摆脱命令行依赖，无需额外的PDF处理工具，适合在自动化流程中使用。

通过PdfTocExtractor.Cli，您可以轻松从PDF文档中提取书签和目录结构，生成清晰的导航文档，完美适用于文档处理、内容分析和自动化工作流。

🚀 跨平台、零依赖、极速提取，一切尽在 PdfTocExtractor.Cli！

## ✨ 功能特点

- 📖 从PDF文件提取书签/目录信息
- 📄 支持多种输出格式：Markdown、JSON、XML、纯文本
- 🎯 可配置的导出选项（层级深度、页码格式等）
- 🛠️ 提供诊断工具，帮助排查PDF读取问题
- ⚡ 异步操作支持，高性能处理
- 🚀 支持AOT编译，原生性能无需.NET运行时
- 🌐 跨平台支持：Windows、Linux、macOS

## 📦 安装

### 作为 .NET Global Tool 安装

```bash
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
cd PdfTocExtractor/src/PdfTocExtractor.Cli
dotnet build -c Release
```

## 🚀 使用方法

### 基本用法

```bash
# 基本用法 - 提取PDF目录并保存为Markdown
pdftoc extract document.pdf -o output.md

# 指定输出格式
pdftoc extract document.pdf -o output.json -f json

# 设置最大层级深度
pdftoc extract document.pdf -o output.xml --max-depth 3
```

### 高级用法

```bash
# 自定义标题和页码格式
pdftoc extract document.pdf -o output.txt --title "我的文档目录" --page-format "第 {0} 页"

# 包含页码和链接信息
pdftoc extract document.pdf -o output.md --include-pages --include-links

# 显示详细输出
pdftoc extract document.pdf -o output.md --verbose

# 诊断PDF文件问题（当遇到读取错误时很有用）
pdftoc diagnose document.pdf
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

### 诊断命令(diagnose)

| 参数 | 说明 | 是否必需 |
| --- | --- | --- |
| `pdf-file` | 要诊断的PDF文件路径 | 是 |

## 📄 支持的输出格式

- **Markdown** (`md`, `markdown`) - 适合文档和网页显示，支持层级结构
- **JSON** (`json`) - 适合程序处理和API集成，包含完整元数据
- **XML** (`xml`) - 结构化数据交换，标准化格式
- **Text** (`txt`, `text`) - 纯文本格式，简洁易读

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

## 🔧 故障排除

### 常见问题

#### "PdfEncryption exception" 错误

如果遇到此错误，通常是因为PDF文件使用了加密或权限保护。请尝试以下解决方案：

1. **使用诊断命令**：
   ```bash
   pdftoc diagnose your-document.pdf
   ```
   这会显示PDF文件的详细信息，帮助诊断问题。

2. **PDF文件类型**：
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

PdfTocExtractor.Cli 使用以下技术：

- **.NET 8.0** - 现代化的.NET平台
- **[System.CommandLine 2.0.0](https://github.com/dotnet/command-line-api)** - 命令行参数解析
- **[PdfTocExtractor](https://www.nuget.org/packages/PdfTocExtractor)** - 核心PDF处理库
- **AOT编译支持** - 原生性能，无需.NET运行时

## 📄 许可证

MIT License

## 🤝 相关项目

- [PdfTocExtractor](https://www.nuget.org/packages/PdfTocExtractor) - 核心库，用于在.NET项目中集成
- [项目主页](https://github.com/star-plan/pdf-toc-extractor) - 完整的项目文档和示例

## 📞 支持

如果您遇到问题或有建议，请：

- 📋 [提交Issue](https://github.com/star-plan/pdf-toc-extractor/issues)
- 💬 [参与讨论](https://github.com/star-plan/pdf-toc-extractor/discussions)
- ⭐ 如果这个项目对您有帮助，请给个Star！
