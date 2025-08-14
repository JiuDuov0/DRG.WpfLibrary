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
1. **文件夹 Folder**  
   文件夹名称为您的 MOD 名称。  
   The folder name should be your MOD name.

2. **Index.html**  
   该文件为您需要在遮罩层显示的内容。  
   This file contains the content you want to display in the overlay.

3. **settings.html**  
   该文件是您需要与 `settings.json` 进行交互的内容。  
   This file is used to interact with `settings.json`.

4. **settings.json**  
   该文件是您需要与 `settings.html` 进行交互的内容。`x` 和 `y` 是位置信息，`width` 和 `height` 是宽高信息。这 4 项如果您有 `index.html` 则必须填写，否则不需要填写。  
   This file is used to interact with `settings.html`.  
   `x` and `y` are position information; `width` and `height` are size information.  
   If you have `index.html`, these four items must be filled in; otherwise, they are not required.

## 开发环境  
## Development Environment

- 推荐使用 [Visual Studio](https://visualstudio.microsoft.com/) 进行开发和调试。  
- It is recommended to use [Visual Studio](https://visualstudio.microsoft.com/) for development and debugging.
