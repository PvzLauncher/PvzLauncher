using Newtonsoft.Json;
using System.Drawing;

namespace PvzLauncherRemake.Classes.JsonConfigs
{
    public class JsonGameInfo
    {
        public partial class Root
        {
            [JsonProperty("tip")]
            public string Tip { get; set; } = "此文件为PvzLauncher版本标志文件，请勿移除！";

            [JsonProperty("game_info")]
            public GameInfo GameInfo { get; set; }

            [JsonProperty("config")]
            public Config Config { get; set; } = new Config();

            [JsonProperty("record")]
            public Record Record { get; set; }
        }

        public partial class GameInfo
        {
            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("icon")]
            public string Icon { get; set; }

            [JsonProperty("is_favorite")]
            public bool IsFavorite { get; set; } = false;

            [JsonProperty("execute_name")]
            public string ExecuteName { get; set; }

            [JsonProperty("game_path")]
            public string? GamePath { get; set; } = null;//普通游戏不写，仅虚拟导入
        }

        public partial class Config
        {
            [JsonProperty("slot_positions")]
            public Dictionary<int, Point> SlotPositions { get; set; } = new Dictionary<int, Point>()
            {
                [1] = new Point(0, 0),
                [2] = new Point(0, 0),
                [3] = new Point(0, 0),
                [4] = new Point(0, 0),
                [5] = new Point(0, 0),
                [6] = new Point(0, 0),
                [7] = new Point(0, 0),
                [8] = new Point(0, 0),
                [9] = new Point(0, 0),
                [10] = new Point(0, 0),

                [0] = new Point(0, 0),//shovel
            };
        }

        public partial class Record
        {
            [JsonProperty("first_play")]
            public long FirstPlay { get; set; }

            [JsonProperty("play_time")]
            public long PlayTime { get; set; }

            [JsonProperty("play_count")]
            public long PlayCount { get; set; }
        }
    }
}
