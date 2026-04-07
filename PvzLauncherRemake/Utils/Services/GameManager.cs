using HuaZi.Library.Downloader;
using HuaZi.Library.Json;
using Microsoft.Win32;
using ModernWpf.Controls;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using PvzLauncherRemake.Utils.Configuration;
using PvzLauncherRemake.Utils.FileSystem;
using PvzLauncherRemake.Utils.UI;
using PvzLauncherRemake.Windows;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static PvzLauncherRemake.Classes.AppLogger;

namespace PvzLauncherRemake.Utils.Services
{
    public static class GameManager
    {
        public static DateTimeOffset? LatestGameLaunchTime = null;
        public static bool IsGameRuning = false;

        #region 加载列表

        /// <summary>
        /// 加载游戏列表
        /// </summary>
        /// <returns>无</returns>
        public static async Task LoadGameListAsync()
        {
            logger.Info("[游戏管理器] 开始加载游戏版本列表");

            var validGames = new List<JsonGameInfo.Index>();

            foreach (string dir in Directory.EnumerateDirectories(AppGlobals.Directories.GameDirectory))
            {
                string configPath = Path.Combine(dir, ".pvzl.json");
                if (!File.Exists(configPath)) continue;

                try
                {
                    var config = Json.ReadJson<JsonGameInfo.Index>(configPath);
                    if (config != null)
                    {
                        if (AppGlobals.Config.Settings.SaveConfig.EnableSaveIsolation)
                        {
                            string saveDir = Path.Combine(dir, ".save");
                            if (!Directory.Exists(saveDir))
                                Directory.CreateDirectory(saveDir);
                        }

                        validGames.Add(config);
                    }
                    else
                    {
                        logger.Warn($"[游戏管理器] 配置文件为空 {configPath}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"[游戏管理器] 读取游戏配置文件失败，已跳过: {configPath}\n{ex.Message}");
                }
            }

            AppGlobals.Indexes.GameList = validGames;
            logger.Info($"[游戏管理器] 加载游戏版本完成，共 {AppGlobals.Indexes.GameList.Count} 个有效版本");
        }

        /// <summary>
        /// 加载修改器列表
        /// </summary>
        /// <returns>无</returns>
        public static async Task LoadTrainerListAsync()
        {
            logger.Info("[游戏管理器] 开始加载修改器版本列表");

            var validTrainers = new List<JsonTrainerInfo.Index>();

            foreach (string dir in Directory.EnumerateDirectories(AppGlobals.Directories.TrainerDirectory))
            {
                string configPath = Path.Combine(dir, ".pvzl.json");
                if (!File.Exists(configPath)) continue;

                try
                {
                    var config = Json.ReadJson<JsonTrainerInfo.Index>(configPath);
                    if (config != null)
                    {
                        validTrainers.Add(config);
                    }
                    else
                    {
                        logger.Warn($"[游戏管理器] 配置文件为空 {configPath}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"[游戏管理器] 读取游戏配置文件失败，已跳过: {configPath}\n{ex.Message}");
                }
            }

            AppGlobals.Indexes.TrainerList = validTrainers;
            logger.Info($"[游戏管理器] 加载游戏版本完成，共 {AppGlobals.Indexes.TrainerList.Count} 个有效版本");
        }

        #endregion

        #region 导入游戏

        /// <summary>
        /// 导入游戏或修改器
        /// </summary>
        /// <returns></returns>
        public static async Task ImportGameOrTrainer(Action<string>? progressCallback = null)
        {
            try
            {
                bool? isTrainer = null;

                //选择位置
                var openFolderDialog = new OpenFolderDialog
                {
                    Multiselect = false,
                    Title = "请选择游戏/修改器所在的文件夹"
                };

        
                //选择类型
                var radioButtonGame = new RadioButton { Content = "游戏" };
                var radioButtonTrainer = new RadioButton { Content = "修改器" };
                radioButtonGame.Click += ((s, e) => isTrainer = false);
                radioButtonTrainer.Click += ((s, e) => isTrainer = true);
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "请选择类型",
                    Content = new StackPanel
                    {
                        Children = { radioButtonGame, radioButtonTrainer }
                    },
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消导入",
                    DefaultButton = ContentDialogButton.Primary
                }, closeCallback: (() => isTrainer = null));

                if (isTrainer == null)
                    return;

                if (openFolderDialog.ShowDialog() != true)
                    return;


                //特殊文件夹判断
                if (openFolderDialog.FolderName == AppGlobals.Directories.ExecuteDirectory ||
                    openFolderDialog.FolderName == AppGlobals.Directories.RootDirectory ||
                    openFolderDialog.FolderName == AppGlobals.Directories.GameDirectory ||
                    openFolderDialog.FolderName == AppGlobals.Directories.TrainerDirectory)
                {
                    SnackbarManager.Show(new SnackbarContent
                    {
                        Title = "导入失败",
                        Content = $"\"{openFolderDialog.FolderName}\" 是一个非法路径，请重新导入！",
                        Type = SnackbarType.Error
                    });
                    return;
                }


                //解决重名
                string? savePath = await ResolveSameName(Path.GetFileName(openFolderDialog.FolderName), (isTrainer == true ? AppGlobals.Directories.TrainerDirectory : AppGlobals.Directories.GameDirectory));

                if (string.IsNullOrEmpty(savePath))
                    return;

                //解决多exe
                string? exeFile = null;
                string[] files = Directory.GetFiles(openFolderDialog.FolderName);
                var listBox = new ListBox();

                foreach (var file in files)
                {
                    if (file.EndsWith(".exe"))
                    {
                        listBox.Items.Add(Path.GetFileName(file));
                    }
                }

                if (listBox.Items.Count == 1)
                {
                    exeFile = (string)listBox.Items[0];
                }
                else if (listBox.Items.Count <= 0)
                {
                    SnackbarManager.Show(new SnackbarContent
                    {
                        Title = "导入终止",
                        Content = "您选择的文件夹内没有任何可执行文件，导入被终止",
                        Type = SnackbarType.Error
                    });
                    return;
                }
                else
                {
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "帮助我们解决问题！",
                        Content = new StackPanel
                        {
                            Children =
                        {
                            new TextBlock{Text="我们在您的文件夹内发现了多个可执行文件！请帮助我们选择正确的那一个！",Margin=new Thickness(0,0,0,10)},
                            listBox
                        }
                        },
                        PrimaryButtonText = "确定",
                        CloseButtonText = "取消导入",
                        DefaultButton = ContentDialogButton.Primary
                    });
                    if (listBox.SelectedItem == null)
                        return;

                    exeFile = (string)listBox.SelectedItem;
                }

                //导入确认
                bool isImportConfirm = false;
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "导入确认",
                    Content = "此操作会将您选择的文件夹复制到启动器游戏库内，如果游戏过可能会需要很长时间，且此操作无法取消！\n\n确定开始导入？",
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                }, (() => isImportConfirm = true));

                if (!isImportConfirm)
                    return;

                await DirectoryManager.CopyDirectoryAsync(openFolderDialog.FolderName, savePath, ((p) => progressCallback?.Invoke(p)));



                if (isTrainer == true)
                {
                    var config = new JsonTrainerInfo.Index
                    {
                        ExecuteName = exeFile,
                        Icon = "origin",
                        Name = Path.GetFileName(savePath),
                        Version = "1.0.0.0"
                    };
                    Json.WriteJson(Path.Combine(savePath, ".pvzl.json"), config);
                }
                else
                {
                    var config = new JsonGameInfo.Index
                    {
                        GameInfo = new JsonGameInfo.GameInfo
                        {
                            ExecuteName = exeFile,
                            Icon = "origin",
                            Name = Path.GetFileName(savePath),
                            Version = "1.0.0.0",
                        },
                        Record = new JsonGameInfo.Record
                        {
                            FirstPlay = DateTimeOffset.Now.ToUnixTimeSeconds(),
                            PlayCount = 0,
                            PlayTime = 0
                        }
                    };
                    Json.WriteJson(Path.Combine(savePath, ".pvzl.json"), config);
                }

                SnackbarManager.Show(new SnackbarContent
                {
                    Title = "导入",
                    Content = $"导入 \"{Path.GetFileName(savePath)}\" 成功！",
                    Type = SnackbarType.Success
                });



                return;


            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }

        #endregion

        #region 下载

        public static async Task StartDownloadAsync(dynamic info, string savePath, bool isTrainer)
        {
            string tempPath = Path.Combine(AppGlobals.Directories.TempDiectory, $"PVZLAUNCHER.DOWNLOAD.CACHE.{new Random().Next(Int32.MinValue, Int32.MaxValue) + new Random().Next(Int32.MinValue, Int32.MaxValue)}");

            logger.Info($"[下载] 生成随机临时名: {tempPath}");

            try
            {
                //清除残留
                if (File.Exists(tempPath))
                    await Task.Run(() => File.Delete(tempPath));

                //定义下载器
                TaskManager.AddTask(new DownloadTaskInfo
                {
                    Downloader = new Downloader
                    {
                        Url = info.Url,
                        SavePath = tempPath
                    },
                    GameInfo = isTrainer ? null : info,
                    TrainerInfo = isTrainer ? info : null,
                    TaskName = $"下载 {Path.GetFileName(savePath)}",
                    TaskType = isTrainer ? TaskType.Trainer : TaskType.Game,
                    SavePath = savePath,
                    TaskIcon = GameIconConverter.ParseStringToGameIcons(info.Icon)
                });
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }

        #endregion

        #region 启动/等待/结束游戏

        /// <summary>
        /// 启动游戏
        /// </summary>
        /// <param name="gameInfo">要启动的游戏信息</param>
        public static async void LaunchGame(JsonGameInfo.Index gameInfo, Action? exitCallback = null)
        {
            //游戏exe路径
            string gameExePath = System.IO.Path.Combine(AppGlobals.Directories.GameDirectory, gameInfo.GameInfo.Name, gameInfo.GameInfo.ExecuteName);
            logger.Info($"[游戏管理器] 设置游戏可执行文件路径: {gameExePath}");
            //定义Process
            AppProcess.Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = gameExePath,
                    UseShellExecute = true,
                    WorkingDirectory = System.IO.Path.Combine(AppGlobals.Directories.GameDirectory, gameInfo.GameInfo.Name)
                }
            };
            logger.Info($"[游戏管理器] 启动进程");
            //启动
            AppProcess.Process.Start();

            LatestGameLaunchTime = DateTimeOffset.Now;

            IsGameRuning = true;

            //启动后操作
            logger.Info($"[启动] 启动后操作为: {AppGlobals.Config.Settings.LauncherConfig.LaunchedOperate}");
            switch (AppGlobals.Config.Settings.LauncherConfig.LaunchedOperate)
            {
                case "Close":
                    Environment.Exit(0); break;
                case "HideAndDisplay":
                    Application.Current.MainWindow.Visibility = Visibility.Hidden; break;
            }
            SetGameFullScreen();
            SetGameLocation();
            Set3DMode();
            if (!string.IsNullOrEmpty(AppGlobals.Config.Settings.GameConfig.WindowTitle))
                SetGameTitle(AppGlobals.Config.Settings.GameConfig.WindowTitle);
            if (AppGlobals.Config.Settings.GameConfig.OverlayUIEnabled)
            {
                var windowOverlay = new WindowOverlay();
                windowOverlay.Show();
            }

            //启动次数
            gameInfo.Record.PlayCount++;
            logger.Info($"[启动] 启动次数+1, 现在为： {gameInfo.Record.PlayCount}");
            Json.WriteJson(System.IO.Path.Combine(AppGlobals.Directories.GameDirectory, gameInfo.GameInfo.Name, ".pvzl.json"), gameInfo);

            //启动器整体次数
            AppGlobals.Config.Record.LaunchCount++;
            ConfigManager.SaveConfig();
            logger.Info($"[启动] 启动器总体启动数: {AppGlobals.Config.Record.LaunchCount}");

            logger.Info($"[启动] 启动操作完毕，等待游戏结束...");
            await WaitGameExit(gameInfo);

            exitCallback?.Invoke();
        }

        /// <summary>
        /// 等待游戏退出
        /// </summary>
        public static async Task WaitGameExit(JsonGameInfo.Index gameInfo)
        {
            await AppProcess.Process.WaitForExitAsync();

            IsGameRuning = false;

            logger.Info($"[游戏管理器] 启动后操作为: {AppGlobals.Config.Settings.LauncherConfig.LaunchedOperate}");
            switch (AppGlobals.Config.Settings.LauncherConfig.LaunchedOperate)
            {
                case "HideAndDisplay":
                    Application.Current.MainWindow.Visibility = Visibility.Visible; break;
            }


            //保存游玩时间
            gameInfo.Record.PlayTime = gameInfo.Record.PlayTime + ((long)(DateTimeOffset.Now - LatestGameLaunchTime!).Value.TotalSeconds);
            Json.WriteJson(Path.Combine(AppGlobals.Directories.GameDirectory, gameInfo.GameInfo.Name, ".pvzl.json"), gameInfo);
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        /// <returns></returns>
        public static async Task KillGame(Action? completeCallback = null, Action? failCallback = null)
        {
            if (!AppProcess.Process.HasExited)
            {
                logger.Info($"[游戏管理器] 尝试使游戏进程自行退出");
                AppProcess.Process.CloseMainWindow();
                //等待自己关闭
                await Task.Delay(1000);

                //强制关
                if (!AppProcess.Process.HasExited)
                {
                    logger.Warn($"[游戏管理器] 游戏进程仍然运行，开始强制结束");
                    AppProcess.Process.Kill();
                    //等待完全关闭
                    await Task.Delay(1000);
                }

                if (!AppProcess.Process.HasExited)
                {
                    //都Kill()了不能再关不上吧
                    logger.Error($"[游戏管理器] 无法终止游戏进程!");
                    failCallback?.Invoke();
                    return;
                }
                else
                {
                    completeCallback?.Invoke();
                    return;
                }
            }
        }

        #endregion

        #region 存档控制

        /// <summary>
        /// 切换当前存档为当前游戏的独立存档
        /// </summary>
        /// <returns></returns>
        public static async Task SwitchGameSave(JsonGameInfo.Index gamInfo)
        {
            if (Directory.Exists(AppGlobals.Directories.SaveDirectory))
                Directory.Delete(AppGlobals.Directories.SaveDirectory, true);
            await DirectoryManager.CopyDirectoryAsync(Path.Combine(AppGlobals.Directories.GameDirectory, gamInfo.GameInfo.Name, ".save"), AppGlobals.Directories.SaveDirectory);
        }

        /// <summary>
        /// 保存当前存档至独立游戏存档
        /// </summary>
        /// <param name="gamInfo"></param>
        /// <returns></returns>
        public static async Task SaveGameSave(JsonGameInfo.Index gamInfo)
        {
            if (Directory.Exists(Path.Combine(AppGlobals.Directories.GameDirectory, gamInfo.GameInfo.Name, ".save")))
                Directory.Delete(Path.Combine(AppGlobals.Directories.GameDirectory, gamInfo.GameInfo.Name, ".save"), true);
            await DirectoryManager.CopyDirectoryAsync(AppGlobals.Directories.SaveDirectory, Path.Combine(AppGlobals.Directories.GameDirectory, gamInfo.GameInfo.Name, ".save"));
        }

        #endregion

        #region 重名解决

        /// <summary>
        /// 解决重名
        /// </summary>
        /// <param name="name">旧名</param>
        /// <param name="baseDir">基础文件夹</param>
        /// <returns>新名</returns>
        public static async Task<string?> ResolveSameName(string name, string baseDir)
        {
            string path = Path.Combine(baseDir, name);
            if (!Directory.Exists(path)) return path;

            while (true)
            {
                var textBox = new TextBox { Text = name };

                bool isContinue = false;
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "发现重名",
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text=$"在您的库内发现与 \"{name}\" 重名的文件夹, 请输入一个新名称!",
                                Margin=new Thickness(0,0,0,5)
                            },
                            textBox
                        }
                    },
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                }, (() => isContinue = true));

                if (!isContinue)
                    return null;


                if (!Directory.Exists(Path.Combine(baseDir, textBox.Text)))
                    return Path.Combine(baseDir, textBox.Text);
                else
                    SnackbarManager.Show(new SnackbarContent
                    {
                        Title = "无法解决",
                        Content = $"库内仍然有与 \"{textBox.Text}\" 同名的文件夹，请继续解决",
                        Type = SnackbarType.Warn
                    });
            }
        }

        #endregion

        #region 注册表控制

        /// <summary>
        /// 设置游戏屏幕模式
        /// </summary>
        public static void SetGameFullScreen()
        {
            string registyPath = @"SOFTWARE\PopCap\PlantsVsZombies";
            string valueName = "ScreenMode";

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(registyPath))
            {
                int? valueData;
                switch (AppGlobals.Config.Settings.GameConfig.FullScreen)
                {
                    case "FullScreen": valueData = 1; break;
                    case "Windowed": valueData = 0; break;
                    default: valueData = null; break;
                }
                if (valueData != null)
                    key.SetValue(valueName, valueData, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// 设置游戏窗口位置
        /// </summary>
        public static void SetGameLocation()
        {
            string registyPath = @"SOFTWARE\PopCap\PlantsVsZombies";
            string valueXName = "PreferredX";
            string valueYName = "PreferredY";

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(registyPath))
            {
                int? gameWindowX;
                int? gameWindowY;

                switch (AppGlobals.Config.Settings.GameConfig.StartUpLocation)
                {
                    case "Center":
                        gameWindowX = (int)((SystemParameters.WorkArea.Width / 2) - (800 / 2));
                        gameWindowY = (int)((SystemParameters.WorkArea.Height / 2) - (600 / 2));
                        break;
                    case "LeftTop":
                        gameWindowX = 0;
                        gameWindowY = 0;
                        break;

                    default:
                        gameWindowX = null; gameWindowY = null; break;
                }

                if (gameWindowX != null && gameWindowY != null)
                {
                    key.SetValue(valueXName, gameWindowX, RegistryValueKind.DWord);
                    key.SetValue(valueYName, gameWindowY, RegistryValueKind.DWord);
                }
            }
        }

        /// <summary>
        /// 设置游戏3D加速
        /// </summary>
        /// <param name="value"></param>
        public static void Set3DMode()
        {
            string registyPath = @"SOFTWARE\PopCap\PlantsVsZombies";
            string valueName = "Is3D";

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(registyPath))
            {
                int? valueData;
                switch (AppGlobals.Config.Settings.GameConfig.ThreeDMode)
                {
                    case "On": valueData = 1; break;
                    case "Off": valueData = 0; break;
                    default: valueData = null; break;
                }
                if (valueData != null)
                    key.SetValue(valueName, valueData, RegistryValueKind.DWord);
            }
        }

        #endregion

        #region 游戏进程操作
        /// <summary>
        /// 设置全局Process对象的窗口标题
        /// </summary>
        public static async void SetGameTitle(string title, int delayMs = 1000, int retryCount = 10)
        {
            try
            {
                for (int i = 0; i < retryCount; i++)
                {
                    if (!IsGameRuning)
                        return;

                    var result = Win32APIHelper.SetWindowTitle(AppProcess.Process.MainWindowHandle, title);
                    if (result)
                        return;

                    await Task.Delay(delayMs);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"窗口标题设置失败: {ex}");
            }

        }
        #endregion

    }
}