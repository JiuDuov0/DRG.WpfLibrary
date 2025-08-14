# WPF 类库项目 (.NET 9.0)

本项目是一个基于 .NET 9.0 的 WPF 类库。用于为其它 .NET 项目提供 WPF 相关的功能和资源。

## 如何引用本类库

若其它项目需要引用本 WPF 类库，请在被引用项目的 `.csproj` 文件中进行如下设置：

1. **启用 WPF 支持**  
   在 `<PropertyGroup>` 节点中添加：
<UseWPF>true</UseWPF>
2. **设置目标框架**  
   将 `<TargetFramework>` 修改为：
<TargetFramework>net9.0-windows</TargetFramework>
这样配置后，您的项目即可正常引用和使用本 WPF 类库的功能。

## 开发环境

- 推荐使用 [Visual Studio](https://visualstudio.microsoft.com/) 进行开发和调试。
