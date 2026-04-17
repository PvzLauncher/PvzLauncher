using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PvzLauncherRemake.Classes.JsonConfigs
{
    public class JsonFileIndex
    {
        public class Index
        {
            [JsonProperty("file-list")]
            public string[] List { get; set; }

            [JsonProperty("files")]
            public Dictionary<string, FileInfo> Files { get; set; }
        }

        public class FileInfo
        {
            [JsonProperty("original-file-name")]
            public string OriginalFileName { get; set; }

            [JsonProperty("size")]
            public long Size { get; set; }//KiB

            [JsonProperty("url")]
            public string Url { get; set; }
        }
    }
}
