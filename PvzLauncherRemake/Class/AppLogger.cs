using HuaZi.Library.Logger;
using System.IO;

namespace PvzLauncherRemake.Class
{
    public static class AppLogger
    {
        //日志记录器
        public static Logger logger = new Logger
        {
            LogDirectory = Path.Combine(AppGlobals.ExecuteDirectory, "Logs"),
            ShowCallerInfo = false,
            ShowDate = false
        };
    }
}