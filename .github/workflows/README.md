# GitHub Actions 工作流说明

本项目包含两个主要的GitHub Actions工作流：

## 📋 工作流概览

### 1. 持续集成 (CI) - `ci.yaml`
**触发条件**：推送到 `main` 或 `develop` 分支，或创建针对这些分支的Pull Request

**功能**：
- ✅ 代码格式检查
- 🔨 多平台构建测试
- 🧪 单元测试执行
- 📊 代码覆盖率收集
- 🚀 AOT编译测试（Linux、Windows、macOS）

### 2. 发布流程 (Publish) - `publish.yaml`
**触发条件**：推送版本标签（格式：`v*.*.*`，如 `v1.0.0`）

**功能**：
- 🧪 运行完整测试套件
- 📦 发布NuGet包（核心库 + CLI工具）
- 🔨 多平台AOT编译（Windows、Linux、macOS）
- 📋 创建GitHub Release
- 📁 上传可执行文件到Release

## 🚀 发布新版本

### 步骤1：准备发布
1. 确保所有测试通过
2. 更新版本号（在项目文件中）
3. 更新CHANGELOG.md（如果有）

### 步骤2：创建并推送标签
```bash
# 创建标签
git tag v1.0.0

# 推送标签到远程仓库
git push origin v1.0.0
```

### 步骤3：自动化流程
推送标签后，GitHub Actions将自动：
1. 运行测试
2. 发布NuGet包
3. 编译多平台可执行文件
4. 创建GitHub Release

## 🔧 配置要求

### 必需的Secrets
在GitHub仓库设置中添加以下Secrets：

- `NUGET_GALLERY_TOKEN`：NuGet.org的API密钥
  - 获取方式：登录 [NuGet.org](https://www.nuget.org) → Account Settings → API Keys

### 权限设置
工作流需要以下权限（已在YAML中配置）：
- `contents: write` - 创建Release
- `id-token: write` - 身份验证
- `issues: write` - 更新Issue

## 📦 发布产物

### NuGet包
- `PdfTocExtractor` - 核心库
- `PdfTocExtractor.Cli` - CLI工具包

### 可执行文件
- `PdfTocExtractor-windows-{version}.zip` - Windows可执行文件
- `PdfTocExtractor-linux-{version}.tar.gz` - Linux可执行文件
- `PdfTocExtractor-macOS-{version}.tar.gz` - macOS可执行文件

## 🔍 监控和调试

### 查看工作流状态
1. 访问仓库的 "Actions" 标签页
2. 选择相应的工作流运行
3. 查看详细日志

### 常见问题
1. **AOT编译失败**：检查代码是否AOT兼容
2. **NuGet发布失败**：验证API密钥是否正确
3. **测试失败**：确保所有测试在本地通过

## 📝 工作流特性

### 优化特性
- ✅ NuGet包缓存，提升构建速度
- ✅ 并行构建多个平台
- ✅ 失败时不影响其他平台构建
- ✅ 自动生成Release说明

### 安全特性
- ✅ 最小权限原则
- ✅ 安全的密钥管理
- ✅ 构建产物验证

## 🎯 下一步优化

可考虑的改进：
- 添加安全扫描
- 集成代码质量检查工具
- 添加性能基准测试
- 支持预发布版本
