using Newtonsoft.Json;

namespace PvzLauncherRemake.JsonModel.Local
{
    public class JsonTrainerProfile
    {
        public class Root
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("execute_name")]
            public string ExecuteName { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("icon")]
            public string Icon { get; set; }
        }
    }
}
