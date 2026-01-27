using Newtonsoft.Json;

namespace PvzLauncherRemake.Class.JsonConfigs
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
