# WPF 类库项目 (.NET 9.0)
# WPF Library Project (.NET 9.0)

本项目是一个基于 .NET 9.0 的 WPF 类库。用于为其它 .NET 项目提供 WPF 相关的功能和资源。  
This project is a WPF library based on .NET 9.0, providing WPF-related features and resources for other .NET projects.

## 如何引用本类库  
## How to Reference This Library

若其它项目需要引用本 WPF 类库，请在被引用项目的 `.csproj` 文件中进行如下设置：  
To reference this WPF library in another project, please configure the target project's `.csproj` file as follows:

1. **启用 WPF 支持**  
   在 `<PropertyGroup>` 节点中添加：  
   **Enable WPF support**  
   Add the following to the `<PropertyGroup>` node:

```
<UseWPF>true</UseWPF>
```
2. **设置目标框架**  
   将 `<TargetFramework>` 修改为：  
   **Set the target framework**  
   Change `<TargetFramework>` to:
```
<TargetFramework>net9.0-windows</TargetFramework>
```
这样配置后，您的项目即可正常引用和使用本 WPF 类库的功能。  
After this configuration, your project can reference and use the WPF library normally.

## 引用示例  
## Reference Example
```
using DRG.WpfLibrary.Demo;

const int maxRetries = 3;
int attempt = 0;
bool success = false;

while (attempt < maxRetries && !success)
{
    try
    {
        _ = new Start();
        success = true;
    }
    catch (Exception)
    {
        attempt++;
    }
}
```

这样即可在您的项目中正常引用和使用 WPF 类库的功能。  
This allows you to reference and use the WPF library in your project.


## Overlay文件夹说明  
## About the Overlay Folder

Overlay 文件夹包含了 WPF 类库的 Overlay 相关资源和代码。需添加到您的项目的\bin\Debug\net9.0-windows下。例如：
The Overlay folder contains resources and code related to the WPF library's Overlay feature. It should be added to your project's `\bin\Debug\net9.0-windows` directory. For example:
```
x:\Code\YourProject\bin\Debug\net9.0-windows
```

## 开发环境  
## Development Environment

- 推荐使用 [Visual Studio](https://visualstudio.microsoft.com/) 进行开发和调试。  
- It is recommended to use [Visual Studio](https://visualstudio.microsoft.com/) for development and debugging.
