using GameLibraryEditor.JsonConfigs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameLibraryEditor
{
    public static class Globals
    {
        public static readonly string ExecuteDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public static readonly string ConfigPath = Path.Combine(ExecuteDirectory, "config.json");

        public static JsonConfig.Root Config;
    }
}
