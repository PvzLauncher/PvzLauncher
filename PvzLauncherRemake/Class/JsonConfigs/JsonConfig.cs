using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvzLauncherRemake.Class.JsonConfigs
{
    public class JsonConfig
    {
        public class Index
        {
            [JsonProperty("current_game")]
            public string CurrentGame { get; set; } = null!;

            [JsonProperty("launcher_config")]
            public LauncherConfig LauncherConfig { get; set; } = new LauncherConfig();
        }

        public class LauncherConfig
        {
            [JsonProperty("launched_operate")]
            public string LaunchedOperate { get; set; } = "None";

            [JsonProperty("window_title")]
            public string WindowTitle { get; set; } = "Plants Vs. Zombies Launcher";

            [JsonProperty("title_image")]
            public string TitleImage { get; set; } = "EN";
        }
    }
}
