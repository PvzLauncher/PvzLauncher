using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
