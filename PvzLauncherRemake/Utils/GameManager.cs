using HuaZi.Library.Json;
using ModernWpf.Controls;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class GameManager
    {
        /// <summary>
        /// 加载游戏列表
        /// </summary>
        /// <returns>无</returns>
        public static async Task LoadGameListAsync()
        {
            logger.Info("[游戏管理器] 开始加载游戏版本列表");

            var validGames = new List<JsonGameInfo.Index>();

            foreach (string dir in Directory.EnumerateDirectories(AppInfo.GameDirectory))
            {
                string configPath = Path.Combine(dir, ".pvzl.json");
                if (!File.Exists(configPath)) continue;

                try
                {
                    var config = Json.ReadJson<JsonGameInfo.Index>(configPath);
                    if (config != null)
                    {
                        if (AppInfo.Config.SaveConfig.EnableSaveIsolation)
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

            AppInfo.GameList = validGames;
            logger.Info($"[游戏管理器] 加载游戏版本完成，共 {AppInfo.GameList.Count} 个有效版本");
        }

        /// <summary>
        /// 加载修改器列表
        /// </summary>
        /// <returns>无</returns>
        public static async Task LoadTrainerListAsync()
        {
            logger.Info("[游戏管理器] 开始加载修改器版本列表");

            var validTrainers = new List<JsonTrainerInfo.Index>();

            foreach (string dir in Directory.EnumerateDirectories(AppInfo.TrainerDirectory))
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

            AppInfo.TrainerList = validTrainers;
            logger.Info($"[游戏管理器] 加载游戏版本完成，共 {AppInfo.TrainerList.Count} 个有效版本");
        }

        /// <summary>
        /// 解决重名
        /// </summary>
        /// <param name="name">旧名</param>
        /// <param name="baseDir">基础文件夹</param>
        /// <returns>新名</returns>
        public static async Task<string> ResolveSameName(string name, string baseDir)
        {
            string path = Path.Combine(baseDir, name);
            if (!Directory.Exists(path)) return path;

            while (true)
            {
                var textBox = new TextBox { Text = name };
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
                    DefaultButton = ContentDialogButton.Primary
                });

                if (!Directory.Exists(Path.Combine(baseDir, textBox.Text)))
                    return Path.Combine(baseDir, textBox.Text);
                else
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "无法解决",
                        Message = $"库内仍然有与 \"{textBox.Text}\" 同名的文件夹，请继续解决",
                        Type = NotificationType.Warning
                    });
            }
        }
    }
}