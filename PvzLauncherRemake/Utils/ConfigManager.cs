using HuaZi.Library.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using System.IO;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class ConfigManager
    {
        public static void SaveAllConfig()
        {
            Json.WriteJson(Path.Combine(AppInfo.ExecuteDirectory, "config.json"), AppInfo.Config);
            logger.Info($"[配置管理器] 保存所有配置");
        }

        public static void ReadAllConfig()
        {
            AppInfo.Config = Json.ReadJson<JsonConfig.Index>(Path.Combine(AppInfo.ExecuteDirectory, "config.json"));
            logger.Info($"[配置管理器] 读取所有配置");
        }
    }
}
