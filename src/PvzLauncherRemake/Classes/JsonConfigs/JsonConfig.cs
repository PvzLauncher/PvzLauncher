using Newtonsoft.Json;
using System.Drawing;

namespace PvzLauncherRemake.Classes.JsonConfigs
{
    public class JsonConfig
    {
        public class Root
        {
            [JsonProperty("current_game")]
            public string CurrentGame { get; set; } = null!;

            [JsonProperty("current_trainer")]
            public string CurrentTrainer { get; set; } = null!;

            [JsonProperty("eula")]
            public bool Eula { get; set; } = false;

            [JsonProperty("window_size")]
            public WindowSize WindowSize { get; set; } = new WindowSize();

            [JsonProperty("settings")]
            public Settings Settings { get; set; } = new Settings();

            [JsonProperty("overlay_window_settings")]
            public OverlayWindowSettings OverLayWindowSettings { get; set; } = new OverlayWindowSettings();

            [JsonProperty("record")]
            public Record Record { get; set; } = new Record();
        }

        public class Settings
        {
            [JsonProperty("launcher_config")]
            public LauncherConfig LauncherConfig { get; set; } = new LauncherConfig();

            [JsonProperty("save_config")]
            public SaveConfig SaveConfig { get; set; } = new SaveConfig();

            [JsonProperty("game_config")]
            public GameConfig GameConfig { get; set; } = new GameConfig();
        }

        public class LauncherConfig
        {
            [JsonProperty("launched_operate")]
            public string LaunchedOperate { get; set; } = "None";

            [JsonProperty("launch_with_trainer")]
            public bool LaunchWithTrainer { get; set; } = false;

            [JsonProperty("manage_select_mode")]
            public string ManageSelectMode { get; set; } = "Single";

            [JsonProperty("theme")]
            public string Theme { get; set; } = "Light";

            [JsonProperty("language")]
            public string Language { get; set; } = "zh-CN";

            [JsonProperty("window_title")]
            public string WindowTitle { get; set; } = "Plants Vs. Zombies Launcher";

            [JsonProperty("title_image")]
            public string TitleImage { get; set; } = "EN";

            [JsonProperty("background_mode")]
            public string BackgroundMode { get; set; } = "default";

            [JsonProperty("navigation_view_align")]
            public string NavigationViewAlign { get; set; } = "Top";

            /*[JsonProperty("echo_cave_enabled")]
            public bool EchoCaveEnabled { get; set; } = true;*/

            [JsonProperty("notice_enabled")]
            public bool NoticeEnabled { get; set; } = true;

            [JsonProperty("launch_animation_enabled")]
            public bool LaunchAnimationEnabled { get; set; } = true;

            [JsonProperty("service_provider")]
            public string ServiceProvider { get; set; } = "Gitee";//Gitee GitCode Github

            [JsonProperty("offline_mode")]
            public bool OfflineMode { get; set; } = false;

            [JsonProperty("update_channel")]
            public string UpdateChannel { get; set; } = Globals.IsStable ? "Stable" : "Development";

            [JsonProperty("start_up_check_update")]
            public bool StartUpCheckUpdate { get; set; } = true;

            [JsonProperty("hidden_notices")]
            public List<string> HiddenNotices { get; set; } = new List<string>();
        }

        public class SaveConfig
        {
            [JsonProperty("enable_save_isolation")]
            public bool EnableSaveIsolation { get; set; } = false;
        }

        public class WindowSize
        {
            [JsonProperty("width")]
            public double Width { get; set; } = 1000;

            [JsonProperty("height")]
            public double Height { get; set; } = 600;
        }

        public class GameConfig
        {
            [JsonProperty("full_screen")]
            public string FullScreen { get; set; } = "Default";

            [JsonProperty("start_up_location")]
            public string StartUpLocation { get; set; } = "Default";

            [JsonProperty("3d_mode")]
            public string ThreeDMode { get; set; } = "Default";

            [JsonProperty("window_title")]
            public string WindowTitle { get; set; } = "";

            [JsonProperty("overlay_enabled")]
            public bool OverlayUIEnabled { get; set; } = false;
        }


        public class OverlayWindowSettings
        {
            [JsonProperty("slot_hotkey_enabled")]
            public bool SlotHotkeyEnabled { get; set; } = false;

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

                [0] = new Point(0, 0),//shovel
            };
        }


        public class Record
        {
            [JsonProperty("launch_count")]
            public int LaunchCount { get; set; } = 0;
        }
    }
}
