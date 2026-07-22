using Newtonsoft.Json;

namespace PvzLauncherRemake.Classes.JsonConfigs
{
    public class JsonUpdateIndex
    {
        public class Root
        {
            [JsonProperty("stable")]
            public ChannelInfo Stable { get; set; }

            [JsonProperty("development")]
            public ChannelInfo Development { get; set; }

            [JsonProperty("new-ui")]
            public ChannelInfo NewUI { get; set; }
        }

        public class ChannelInfo
        {
            [JsonProperty("latest_version")]
            public string LatestVersion { get; set; }

            [JsonProperty("change_log")]
            public string ChangeLog { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("url_shell")]
            public string UrlShell { get; set; }
        }
    }
}
