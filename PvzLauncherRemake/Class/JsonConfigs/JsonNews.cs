using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PvzLauncherRemake.Class.JsonConfigs
{
    public class JsonNews
    {
        public class Index
        {
            [JsonProperty("news")]
            public NewsInfo[] News { get; set; }
        }

        public class NewsInfo
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("date")]
            public string Date { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }
        }
    }
}
