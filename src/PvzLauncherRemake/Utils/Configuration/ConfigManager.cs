using HuaZi.Library.Json;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using System.IO;


namespace PvzLauncherRemake.Utils.Configuration
{
    public static class ConfigManager
    {
        public static void CreateDefaultConfig()
        {
            Globals.Config = new JsonConfig.Root();
            SaveConfig();
        }

        public static void SaveConfig() => Json.WriteJson(Globals.Paths.ConfigPath, Globals.Config);

        public static void LoadConfig()
        {
            try
            {
                if (!File.Exists(Globals.Paths.ConfigPath))
                {
                    CreateDefaultConfig();

                    return;
                }

                var config = Json.ReadJson<JsonConfig.Root>(Globals.Paths.ConfigPath);
                if (config == null)
                {

                    CreateDefaultConfig();
                    return;
                }
                Globals.Config = config;

            }
            catch (Exception)
            {
                CreateDefaultConfig();
            }
        }
    }
}
