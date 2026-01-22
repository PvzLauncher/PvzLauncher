using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PvzLauncherRemake.Class.JsonConfigs
{
    public class JsonHelpIndex
    {
        public class Index
        {
            [JsonProperty("root")]
            public CardInfo[] Root { get; set; }
        }

        public class CardInfo
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("subtitle")]
            public string Subtitle { get; set; }

            [JsonProperty("childrens")]
            public CardInfo[] Childrens { get; set; }

            [JsonProperty("content")]
            public ContentInfo[] Content { get; set; }
        }

        public class ContentInfo
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }
        }
    }
}
