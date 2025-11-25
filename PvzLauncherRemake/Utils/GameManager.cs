using HuaZi.Library.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using System.IO;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class GameManager
    {
        /// <summary>
        /// 加载游戏列表
        /// </summary>
        public static async Task LoadGameList()
        {
            await Task.Run(() =>
            {
                logger.Info("开始加载游戏列表");
                //清理
                AppInfo.GameList.Clear();

                //游戏文件夹
                string[] games = Directory.GetDirectories(AppInfo.GameDirectory);

                foreach (var game in games)
                {
                    string configPath = Path.Combine(game, ".pvzl.json");
                    if (File.Exists(configPath))
                    {
                        JsonGameInfo.Index configContent = Json.ReadJson<JsonGameInfo.Index>(configPath);
                        logger.Info($"找到游戏配置: {Path.GetFileName(game)}");
                        AppInfo.GameList.Add(configContent);
                    }
                }

                logger.Info("加载游戏列表结束");
            });
            
        }

        /// <summary>
        /// 加载修改器列表
        /// </summary>
        public static async Task LoadTrainerList()
        {
            await Task.Run(() =>
            {
                logger.Info("开始加载修改器列表");
                //清理
                AppInfo.TrainerList.Clear();

                //游戏文件夹
                string[] trainers = Directory.GetDirectories(AppInfo.TrainerDirectory);

                foreach (var trainer in trainers)
                {
                    string configPath = Path.Combine(trainer, ".pvzl.json");
                    if (File.Exists(configPath))
                    {
                        JsonTrainerInfo.Index configContent = Json.ReadJson<JsonTrainerInfo.Index>(configPath);
                        logger.Info($"找到修改器配置: {Path.GetFileName(trainer)}");
                        AppInfo.TrainerList.Add(configContent);
                    }
                }

                logger.Info("加载修改器列表结束");
            });
        }
    }
}
