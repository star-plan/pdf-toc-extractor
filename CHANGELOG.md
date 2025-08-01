# 更新日志

本文档记录了PdfTocExtractor项目的所有重要更改。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)，
并且本项目遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [未发布]

### 新增
- 初始版本发布
- PDF目录提取核心功能
- 支持多种输出格式：Markdown、JSON、XML、纯文本
- 命令行工具 `pdftoc`
- NuGet包发布：核心库和CLI工具
- AOT编译支持，生成原生可执行文件
- 跨平台支持：Windows、Linux、macOS
- GitHub Actions CI/CD流程
- 可扩展的导出器架构

### 技术特性
- 基于 .NET 8.0
- 使用 iText 9.2.0 进行PDF处理
- 支持异步操作
- 完整的单元测试覆盖
- 代码质量检查和格式化

## [1.0.0] - 即将发布

### 新增
- 🎉 首次正式发布
- 📖 完整的PDF目录提取功能
- 🛠️ 命令行工具和库支持
- 🚀 AOT编译和多平台支持
- 📚 完整的文档和示例

---

## 版本说明

- **主版本号**：当你做了不兼容的 API 修改
- **次版本号**：当你做了向下兼容的功能性新增
- **修订号**：当你做了向下兼容的问题修正

## 贡献指南

如果您想为此项目做出贡献，请：

1. 查看 [Issues](https://github.com/star-plan/PdfTocExtractor/issues) 了解当前的问题和功能请求
2. 提交 Pull Request 时请更新此更新日志
3. 遵循 [语义化版本](https://semver.org/lang/zh-CN/) 规范
