using HuaZi.Library.Downloader;
using HuaZi.Library.Json;
using MdXaml;
using ModernWpf.Controls;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using PvzLauncherRemake.Utils.Configuration;
using PvzLauncherRemake.Utils.UI;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows.Controls;
using System.Windows.Media;


namespace PvzLauncherRemake.Utils.Services
{
    public static class Updater
    {
        public static JsonUpdateIndex.Index UpdateIndex = null!;
        public static HttpClient Client = new HttpClient();

        public static string LatestVersion = null!;
        public static string ChangeLog = null!;
        public static string Url = null!;
        public static string UrlShell = null!;
        public static string BinPackSavePath = Path.Combine(AppGlobals.Directories.TempDiectory, "PVZLAUNCHER.UPDATE.CACHE.BIN");
        public static string ShellPackSavePath = Path.Combine(AppGlobals.Directories.TempDiectory, "PVZLAUNCHER.UPDATE.CACHE.SHELL");

        public static bool isUpdate = false;

        public static async Task CheckUpdate(Action<double, double> progressCallback = null!, bool isStartUp = false)
        {


            if (AppGlobals.Config.Settings.LauncherConfig.OfflineMode)
            {
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "更新不可用",
                    Content = $"离线模式已启用。因此联网功能被禁用，如果你的设备可以正常联网。那么你可以前往设置关闭离线模式",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                });
                return;
            }

            //检查服务可用性
            if (!await CheckService())
            {
                var result = await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "更新服务不可用",
                    Content = "更新服务检查不通过，请检查 .NET 10 Runtime 是否正确安装",
                    PrimaryButtonText = "忽略，继续尝试更新",
                    CloseButtonText = "取消更新",
                    DefaultButton = ContentDialogButton.Close
                });
                if (result == ContentDialogResult.None)
                    return;
            }

            //如是Dev版强制使用Development分支
            if (!AppGlobals.IsStable)
            {
                AppGlobals.Config.Settings.LauncherConfig.UpdateChannel = "Development";
                ConfigManager.SaveConfig();
            }

            //获取主索引
            string indexString = await Client.GetStringAsync(AppGlobals.Urls.UpdateIndexUrl);

            UpdateIndex = Json.ReadJson<JsonUpdateIndex.Index>(indexString);

            //判断更新通道

            switch (AppGlobals.Config.Settings.LauncherConfig.UpdateChannel)
            {
                case "Stable":
                    LatestVersion = UpdateIndex.Stable.LatestVersion;
                    ChangeLog = await Client.GetStringAsync(UpdateIndex.Stable.ChangeLog);
                    Url = UpdateIndex.Stable.Url;
                    UrlShell = UpdateIndex.Stable.UrlShell;
                    break;

                case "Development":
                    LatestVersion = UpdateIndex.Development.LatestVersion;
                    ChangeLog = await Client.GetStringAsync(UpdateIndex.Development.ChangeLog);
                    Url = UpdateIndex.Development.Url;
                    UrlShell = UpdateIndex.Stable.UrlShell;
                    break;

                default:
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "更新终止",
                        Content = $"更新通道 \"{AppGlobals.Config.Settings.LauncherConfig.UpdateChannel}\" 无效！请重新选择有效的更新通道",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    });
                    return;
            }


            //判断版本

            if (AppGlobals.Version != LatestVersion)
            {


                FlowDocumentScrollViewer docViewer = new FlowDocumentScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                };

                docViewer.Document = new Markdown().Transform(ChangeLog);
                docViewer.Document.FontFamily = new FontFamily("Microsoft YaHei UI");

                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = $"发现可用更新 - {LatestVersion}",
                    Content = docViewer,
                    PrimaryButtonText = "立即更新",
                    CloseButtonText = "取消更新",
                    DefaultButton = ContentDialogButton.Primary
                }, (() => { isUpdate = true; }));
            }
            else
            {

                if (!isStartUp)
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "无可用更新",
                        Content = $"您使用的已经是最新版本 {AppGlobals.Version} , 无需更新!",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    });
            }


            //不更新直接return
            if (!isUpdate)
            {

                return;
            }



            //开始更新
            bool? done = null;
            bool? doneShell = null;
            string errorMessage = null!;
            string errorMessageShell = null!;

            var downloader = new Downloader
            {
                Url = Url,
                SavePath = BinPackSavePath,
                Completed = ((s, e) =>
                {
                    if (s)
                        done = true;
                    else
                    {
                        done = false;
                        errorMessage = e!;
                    }
                }),
                Progress = ((p, s) =>
                {
                    progressCallback?.Invoke(p, s);

                })
            };
            var downloaderShell = new Downloader
            {
                Url = UrlShell,
                SavePath = ShellPackSavePath,
                Completed = ((s, e) =>
                {
                    if (s)
                        doneShell = true;
                    else
                    {
                        doneShell = false;
                        errorMessageShell = e!;
                    }
                }),
                Progress = ((p, s) =>
                {
                    progressCallback?.Invoke(p, s);

                })
            };


            downloader.StartDownload();
            downloaderShell.StartDownload();

            //等待下载完毕
            while (done == null || doneShell == null)
                await Task.Delay(1000);


            //如下载失败抛错误
            if (done == false)
                throw new Exception(errorMessage);
            //下载成功↓
            //运行更新服务

            if (File.Exists(Path.Combine(AppGlobals.Directories.ExecuteDirectory, "StdUpdateService.exe")))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine(AppGlobals.Directories.ExecuteDirectory, "StdUpdateService.exe"),
                    Arguments = $"-binpack \"{BinPackSavePath}\" -shellpack \"{ShellPackSavePath}\" -binpath \"{AppGlobals.Directories.ExecuteDirectory}\" -shellpath \"{AppGlobals.Directories.RootDirectory}\" -exepath \"{Path.Combine(AppGlobals.Directories.ExecuteDirectory, "PvzLauncherRemake.exe")}\" -selfupdate",
                    UseShellExecute = true
                });
                Environment.Exit(0);
            }
            else
            {
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "失败",
                    Content = $"无法在 \"{Path.Combine(AppGlobals.Directories.ExecuteDirectory, "UpdateService.exe")}\" 找到更新服务",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                });
                Environment.Exit(1);
            }


        }

        public static async Task<bool> CheckService()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(AppGlobals.Directories.ExecuteDirectory, "StdUpdateService.exe"),
                        Arguments = "-test",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                string output = process.StandardOutput.ReadToEnd();

                await process.WaitForExitAsync();

                return output == "done";
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
