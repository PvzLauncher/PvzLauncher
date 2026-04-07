using HuaZi.Library.Logger;
using System.IO;

namespace PvzLauncherRemake.Classes
{
    public static class AppLogger
    {
        //日志记录器
        public static Logger logger = new Logger
        {
            LogDirectory = Path.Combine(AppGlobals.Directories.ExecuteDirectory, "Logs"),
            ShowCallerInfo = false,
            ShowDate = false
        };
    }
}