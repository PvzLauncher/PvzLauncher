<div align="center">

<img src="docs\image\Icon.png" width="150" height="150" alt="PvzLauncher图标" style="display: block; margin: 0 auto; ">

# Plants Vs. Zombies Launcher

<p style="text-align: center"><i>启动 ·管理 · 下载</i></p>

> 提供丰富的游戏库与高速下载功能。还可以统一管理、启动游戏

</div>

<img src="docs/image/MainWindow.png" alt="主界面" style="display: block; margin: 0 auto; ">

## 官方社群

> [!warning]
> PvzLauncher仅有这一个官方交流社群，如果您进入了其他未经验证的群可能会含有风险

**QQ**: 1040764053

## 💻 兼容情况

|操作系统|支持情况|环境要求|
|-|-|-|
|![win10](/docs/image/Icons/windows.png) Windows 10(1809+) / ![win11](/docs/image/Icons/windows11.png) 11 64-bit                                                                    |✅完全支持     |[.NET 10 Desktop Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0)|
|![win7](/docs/image/Icons/windows7.png) Windows 7 / ![win8](/docs/image/Icons/windows8.png) 8.1 / ![win10-1809](/docs/image/Icons/windows10-1809-.png) 10(1809-)                   |❌不支持       |.NET10 已放弃对这些平台的支持|
|![linux](/docs/image/Icons/linux.png) Linux                                                                                                                                        |❔理论支持     |可以使用Wine此类兼容层运行|
|![android](/docs/image/Icons/android.png) Android|❌不支持|请跳转到安卓版专用仓库：[点我跳转](https://github.com/PvzLauncher/PvzLauncher.Android)|
|![macos](/docs/image/Icons/macos.png) macOS / ![ios](/docs/image/Icons/macos.png) IOS / ![web](/docs/image/Icons/web.png) Web  |❌不支持       |永远也不会支持这些平台|

植物大战僵尸原版**仅支持Windows平台** `(不包含部分跨平台改版)` ，因此启动器也**只支持Windows平台**。同时，游戏库也**不会上架**非Windows平台的游戏

* **✅完全支持**: 程序可以在此平台完美运行，如出现问题会积极解决
* **⚠️部分支持**: 程序可以在这些平台上运行，不过因为某些原因不能完美运行，如果出现一些问题，开发者不会解决
* **❔理论支持**: 程序理论上可以在这些平台上运行，但体验感极差。如果出现问题，开发者不会解决
* **❌不支持**: 程序不可以在这些平台上运行，之后也不会支持

## ✨ 特色功能

### 📥 下载游戏

![download-detail](/docs/image/download-detail.png)

我们拥有一个丰富且持续更新的游戏库。截至本文编写时，游戏库已收录 **40+** 款可下载内容（包括**原版、改版和修改器**）。

所有资源均可通过启动器**免费、高速**下载，并在下载完成后自动解压，无需手动操作。

如果你没有在游戏库中找到想要的改版，欢迎通过 [Issue](https://github.com/PvzLauncher/PvzLauncher/issues/new?template=download.yml) 提交申请，我们会尽力将其添加到游戏库中。

### 📂 导入游戏

![game-manager](/docs/image/game-import.png)

除了直接从启动器下载游戏外，你也可以将本地已有的游戏导入到启动器中进行统一管理。

#### 🔗 虚拟导入

默认情况下，导入游戏会将游戏文件复制到启动器目录。

如果你的游戏体积较大，或希望继续保留原有目录结构，可以选择**虚拟导入**。该模式不会复制任何游戏文件，而是仅创建指向原游戏目录的链接，因此几乎不会额外占用磁盘空间。

## 🚀 快速开始

### 1. 安装 . NET 10 运行时

此程序需要依赖 `.NET 桌面运行时` x64 运行，因此在使用之前你需要前往[此处](https://get.dot.net/10)下载并安装，如果你已经安装，那么可以跳过此步骤

接着，运行下方命令。检查安装是否成功

```bash
dotnet --list-runtimes
```

通常情况下它会输出类似下方的内容

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

确认下载的内容是安全的之后，请使用**任意一款**解压缩软件，解压压缩包内所有文件至**任意文件夹**内

打开目标文件夹，双击运行 `PvzLauncher.exe` 即可。如果您的杀毒软件出现了报毒现象，请不要理睬它。我们的内容是安全的。如果您实在不放心，可以随时停止使用本软件
