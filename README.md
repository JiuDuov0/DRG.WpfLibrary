# WPF �����Ŀ (.NET 9.0)
# WPF Library Project (.NET 9.0)

����Ŀ��һ������ .NET 9.0 �� WPF ��⡣����Ϊ���� .NET ��Ŀ�ṩ WPF ��صĹ��ܺ���Դ��  
This project is a WPF library based on .NET 9.0, providing WPF-related features and resources for other .NET projects.

## ������ñ����  
## How to Reference This Library

��������Ŀ��Ҫ���ñ� WPF ��⣬���ڱ�������Ŀ�� `.csproj` �ļ��н����������ã�  
To reference this WPF library in another project, please configure the target project's `.csproj` file as follows:

1. **���� WPF ֧��**  
   �� `<PropertyGroup>` �ڵ�����ӣ�  
   **Enable WPF support**  
   Add the following to the `<PropertyGroup>` node:

```
<UseWPF>true</UseWPF>
```
2. **����Ŀ����**  
   �� `<TargetFramework>` �޸�Ϊ��  
   **Set the target framework**  
   Change `<TargetFramework>` to:
```
<TargetFramework>net9.0-windows</TargetFramework>
```
�������ú�������Ŀ�����������ú�ʹ�ñ� WPF ���Ĺ��ܡ�  
After this configuration, your project can reference and use the WPF library normally.

## ����ʾ��  
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

����������������Ŀ���������ú�ʹ�� WPF ���Ĺ��ܡ�  
This allows you to reference and use the WPF library in your project.


## Overlay�ļ���˵��  
## About the Overlay Folder

Overlay �ļ��а����� WPF ���� Overlay �����Դ�ʹ��롣����ӵ�������Ŀ��\bin\Debug\net9.0-windows�¡����磺
The Overlay folder contains resources and code related to the WPF library's Overlay feature. It should be added to your project's `\bin\Debug\net9.0-windows` directory. For example:
```
x:\Code\YourProject\bin\Debug\net9.0-windows
```
1. **�ļ��� Folder**  
   �ļ�������Ϊ���� MOD ���ơ�  
   The folder name should be your MOD name.

2. **Index.html**  
   ���ļ�Ϊ����Ҫ�����ֲ���ʾ�����ݡ�  
   This file contains the content you want to display in the overlay.

3. **settings.html**  
   ���ļ�������Ҫ�� `settings.json` ���н��������ݡ�  
   This file is used to interact with `settings.json`.

4. **settings.json**  
   ���ļ�������Ҫ�� `settings.html` ���н��������ݡ�`x` �� `y` ��λ����Ϣ��`width` �� `height` �ǿ����Ϣ���� 4 ��������� `index.html` �������д��������Ҫ��д��  
   This file is used to interact with `settings.html`.  
   `x` and `y` are position information; `width` and `height` are size information.  
   If you have `index.html`, these four items must be filled in; otherwise, they are not required.

## ��������  
## Development Environment

- �Ƽ�ʹ�� [Visual Studio](https://visualstudio.microsoft.com/) ���п����͵��ԡ�  
- It is recommended to use [Visual Studio](https://visualstudio.microsoft.com/) for development and debugging.
