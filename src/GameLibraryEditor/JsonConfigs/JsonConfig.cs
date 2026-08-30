using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameLibraryEditor.JsonConfigs
{
    public class JsonConfig
    {
        public class Root
        {
            [JsonProperty("index_directory")]
            public string IndexDirectory { get; set; } = Globals.ExecuteDirectory;
        }
    }
}
