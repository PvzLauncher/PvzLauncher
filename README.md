<div align="center">

<img src="docs\image\Icon.png" width="150" height="150" alt="PvzLauncher图标" style="display: block; margin: 0 auto; ">

# PvzLauncher

<p style="text-align: center"><i>启动 ·管理 · 下载</i></p>

> 提供丰富的游戏库与高速下载功能。还可以统一管理、启动游戏

</div>

<img src="docs/image/MainWindow.png" alt="主界面" style="display: block; margin: 0 auto; ">

## 🚀 快速开始

### 1. 安装 .NET 10 运行时

此程序需要依赖 .NET 10 Desktop Runtime 运行，因此在使用之前你需要前往[此处](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)下载并安装，如果你已经安装，那么可以跳过此步骤

接着，运行下方命令。检查安装是否成功

``` bash
dotnet --list-runtimes
```

通常情况下他会输出类似下方的内容

```
Microsoft.AspNetCore.App 10.0.x [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
Microsoft.NETCore.App 10.0.x [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
Microsoft.WindowsDesktop.App 10.0.x [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
```

### 2. 下载最新发行版

PvzLauncher有多个更新通道，可供您选择，你可以根据下表来选择自己需要的版本并使用

|更新通道|备注|下载|
|-|-|-|
| `Stable` |稳定版，但更新较慢。追求稳定的可以选择此|[下载](https://github.com/PvzLauncher/PvzLauncher/releases/latest)|
| `Development` |开发版，更新较快。包含了最新的新功能，但可能会出一些BUG。追求抢先体验新功能的可选择此|[下载](https://github.com/PvzLauncher/PvzLauncher/releases)|

### 3. 安装与使用

此软件通过 `.zip` 压缩包方式发布，如过您是通过**其他渠道**下载的，或下载的格式**并非** `.zip` ，<b><u>请不要运行它！</u></b>其内部可能包含**未经验证的危险内容**！

确认下载的内容是安全的之后，请使用**任意一款**解压缩软件，解压下载到的压缩包至**任意文件夹**内

打开目标文件夹，双击运行 `PvzLauncher.exe` 即可运行。如果您的杀毒软件出现了报毒现象，请不要理睬它。我们的内容是安全的。如果您实在不放心，可以随时停止使用本软件

## 💻 兼容情况

|操作系统|支持情况|环境要求|
|-|-|-|
|![win10](/docs/image/Icons/windows.png) Windows 10(1809+) / ![win11](/docs/image/Icons/windows11.png) 11 64-bit|✅完全支持|[.NET 10 Desktop Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0)|
|![win7](/docs/image/Icons/windows7.png) Windows 7 / ![win8](/docs/image/Icons/windows8.png) 8.1 / ![win10-1809](/docs/image/Icons/windows10-1809-.png) 10(1809-) 64-bit|⚠️部分支持|[.NET 10 Desktop Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0)|
|![win32bit](/docs/image/Icons/windows7.png) Windows 32-bit / ARM|❌不支持|仅有x64版本|
|![linux](/docs/image/Icons/linux.png) Linux|❔理论支持|可以使用Wine此类兼容层运行|
|![macos](/docs/image/Icons/macos.png) macOS / ![android](/docs/image/Icons/android.png) Android / ![ios](/docs/image/Icons/macos.png) IOS / ![web](/docs/image/Icons/web.png) Web|❌不支持|永远也不会支持这些平台|

植物大战僵尸原版**仅支持Windows平台** `(不包含部分跨平台改版)` ，因此启动器也**只支持Windows平台**。同时，游戏库也**不会上架**非Windows平台的游戏

* **✅完全支持**: 程序可以在此平台完美运行，如出现问题会积极解决
* **⚠️部分支持**: 程序可以在这些平台上运行，不过因为某些原因不能完美运行，如果出现一些问题，开发者不会解决
* **❔理论支持**: 程序理论上可以在这些平台上运行，但体验感极差。如果出现问题，开发者不会解决
* **❌不支持**: 程序不可以在这些平台上运行，之后也不会支持
