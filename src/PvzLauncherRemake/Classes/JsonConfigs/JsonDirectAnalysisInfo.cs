using Newtonsoft.Json;

namespace PvzLauncherRemake.Classes.JsonConfigs
{
    public class JsonDirectAnalysisInfo
    {
        public class Root
        {
            [JsonProperty("code")]
            public int Code { get; set; }

            [JsonProperty("msg")]
            public string Message { get; set; }

            [JsonProperty("data")]
            public Data Data { get; set; }
        }

        public class Data
        {
            [JsonProperty("directLink")]
            public string DirectLink { get; set; }
        }
    }
}
