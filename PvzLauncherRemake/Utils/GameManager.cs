using HuaZi.Library.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using System.IO;

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
                        
                        AppInfo.GameList.Add(configContent);
                    }
                }

                
            });
            
        }

        /// <summary>
        /// 加载修改器列表
        /// </summary>
        public static async Task LoadTrainerList()
        {
            await Task.Run(() =>
            {
                
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
                        
                        AppInfo.TrainerList.Add(configContent);
                    }
                }

                
            });
        }
    }
}
