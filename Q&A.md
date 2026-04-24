# 常见问题解答 Q&A

#### Q1.在下载游戏时报错 `The SSL connection could not be established, see inner exception.`

此问题通常由系统代理引起，请尝试关闭代理后再次尝试下载游戏

#### Q2. 在下载游戏时报错 `由于目标计算机积极拒绝，无法连接。（lz.quia.top:443）`

此问题由启动器使用的**直链解析服务**异常引起的，请耐心等待服务恢复正常

#### Q3.更新通道改成Development后无法改回Stable

一般来说，由于避免某些bug的产生，我们将测试版更新通道锁定为Development

#### Q4.可以一次性开多个修改器/游戏吗？

游戏不允许多开，原版游戏亦是如此。修改器支持多开

#### Q5.点击启动修改器按钮后修改器未正常启动

有些修改器必须先开游戏本体才能正常启动，检查你是否运行了游戏

#### Q5.You must install or Update.net to run this application

安装.NET Desktop Runtime，参见[仓库主页](https://github.com/PvzLauncher/PvzLauncher)自述文件的 `快速开始` 章节

#### Q6.支持32位系统吗

我们正在逐渐放弃32位系统，但现在你仍然可以在32位系统使用启动器，但更新服务已不可用（锁定为win-x64）

#### Q7.The process cannot access the file 'AppData\Local\Temp\PVZLAUNCHER.UPDATE.CACHE' because it is being used by another process.

更新包文件被异常占用。在点击一次 `检查更新` 后请不要再次点击。
