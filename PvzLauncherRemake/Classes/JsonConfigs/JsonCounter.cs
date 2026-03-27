using Newtonsoft.Json;

namespace PvzLauncherRemake.Classes.JsonConfigs
{
    public class JsonCounter
    {
        public class Index
        {
            [JsonProperty("data")]
            public Data Data { get; set; }
        }

        public class Data
        {
            [JsonProperty("up_count")]
            public long UpCount { get; set; }
        }
    }
}
