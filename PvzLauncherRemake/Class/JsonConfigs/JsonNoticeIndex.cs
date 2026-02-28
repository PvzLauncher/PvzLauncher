using Newtonsoft.Json;

namespace PvzLauncherRemake.Class.JsonConfigs
{
    public class JsonNoticeIndex
    {
        public class Index
        {
            [JsonProperty("notices")]
            public NoticeInfo[] Notices { get; set; }
        }

        public class NoticeInfo
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("contents")]
            public string[] Contents { get; set; }

            [JsonProperty("primary-button")]
            public string PrimaryButton { get; set; }

            [JsonProperty("secondary-button")]
            public string SecondaryButton { get; set; }

            [JsonProperty("primary-actions")]
            public ButtonActionInfo[] PrimaryActions { get; set; }

            [JsonProperty("secondary-actions")]
            public ButtonActionInfo[] SecondaryActions { get; set; }
        }

        public class ButtonActionInfo
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("url")]
            public string? Url { get; set; }
        }
    }
}
