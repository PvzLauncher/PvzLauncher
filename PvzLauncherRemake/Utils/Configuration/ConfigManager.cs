using HuaZi.Library.Json;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using System.IO;


namespace PvzLauncherRemake.Utils.Configuration
{
    public static class ConfigManager
    {
        public static string ConfigPath = Path.Combine(AppGlobals.Directories.ExecuteDirectory, "config.json");

        public static void CreateDefaultConfig()
        {
            AppGlobals.Config = new JsonConfig.Root();
            SaveConfig();
        }

        public static void SaveConfig() => Json.WriteJson(ConfigPath, AppGlobals.Config);

        public static void LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    CreateDefaultConfig();

                    return;
                }

                var config = Json.ReadJson<JsonConfig.Root>(ConfigPath);
                if (config == null)
                {

                    CreateDefaultConfig();
                    return;
                }
                AppGlobals.Config = config;

            }
            catch (Exception)
            {
                CreateDefaultConfig();
            }
        }
    }
}
