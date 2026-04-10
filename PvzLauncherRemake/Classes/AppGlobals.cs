using PvzLauncherRemake.Classes.JsonConfigs;
using System.IO;
using System.Reflection;

namespace PvzLauncherRemake.Classes
{
    //全局类
    public static class AppGlobals
    {
        public static readonly string Version = $"1.5.2-beta.1";//版本
        public static readonly bool IsStable = false;//是否稳定版
        public static JsonConfig.Index Config = null!;//配置

        //路径
        public static class Directories
        {
            public static readonly string ExecuteDirectory = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";//执行目录
            public static readonly string RootDirectory = $"{Path.GetDirectoryName(ExecuteDirectory)}";//顶级目录
            public static readonly string GameDirectory = $"{Path.Combine(ExecuteDirectory, "Games")}";//游戏目录
            public static readonly string TrainerDirectory = $"{Path.Combine(ExecuteDirectory, "Trainer")}";//修改器目录
            public static readonly string TempDiectory = Path.GetTempPath();//临时文件夹
            public static readonly string SaveDirectory = @"C:\ProgramData\PopCap Games\PlantsVsZombies\userdata";//存档文件夹
        }

        //特殊
        public static class Indexes
        {
            public static List<JsonGameInfo.Index> GameList = new List<JsonGameInfo.Index>();//游戏列表
            public static List<JsonTrainerInfo.Index> TrainerList = new List<JsonTrainerInfo.Index>();//修改器

            public static JsonDownloadIndex.Index? DownloadIndex = null;//下载索引
        }

        //字符串
        public static class Urls
        {
            public static readonly string ServiceRootUrl = "https://gitee.com/huamouren110/PvzLauncher.Service/raw/main";//服务根Url
            public static readonly string DownloadIndexUrl = $"{ServiceRootUrl}/game-library/index.json";//下载索引
            public static readonly string UpdateIndexUrl = $"{ServiceRootUrl}/update/latest.json";//更新索引
            public static readonly string NoticeIndexUrl = $"{ServiceRootUrl}/notice/index.json";//公告索引
            public static readonly string FileIndexUrl = $"{ServiceRootUrl}/files/index.json";//文件索引
        }

        //启动参数配置
        public static class Arguments
        {
            public static bool isShell = false;//启动壳启动
            public static bool isUpdate = false;//是否更新完毕启动

            public static bool isCIBuild = false;//是否CI构建
            public static bool isDebugBuild = false;//是调试版构建
        }
    }
}
