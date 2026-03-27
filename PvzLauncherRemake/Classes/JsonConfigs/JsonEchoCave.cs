using Newtonsoft.Json;

namespace PvzLauncherRemake.Classes.JsonConfigs
{
    public class JsonEchoCave
    {
        public class Index
        {
            [JsonProperty("echo-cave")]
            public string[] Data { get; set; }
        }
    }
}
