using PvzLauncherRemake.Classes.JsonConfigs;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace PvzLauncherRemake.Classes
{
    //全局类
    public static class Globals
    {
        public static readonly string Version = $"1.5.4-beta.3";//版本
        public static readonly bool IsStable = false;//是否稳定版
        public static JsonConfig.Root Config = null!;//配置

        //目录
        public static class Directories
        {
            public static readonly string ExecuteDirectory = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";//执行目录

            public static readonly string RootDirectory = $"{Path.Combine(Path.GetDirectoryName(ExecuteDirectory)!, ".pvzl")}";//顶级目录
            public static readonly string GameDirectory = $"{Path.Combine(RootDirectory, "games")}";//游戏目录
            public static readonly string TrainerDirectory = $"{Path.Combine(RootDirectory, "trainer")}";//修改器目录

            public static readonly string TempDiectory = Path.GetTempPath();//临时文件夹
            public static readonly string SaveDirectory = @"C:\ProgramData\PopCap Games\PlantsVsZombies\userdata";//存档文件夹
        }

        //路径
        public static class Paths
        {
            public static readonly string ConfigPath = $"{Path.Combine(Directories.RootDirectory, "config.json")}";
            public static readonly string BackgroundPath = $"{Path.Combine(Directories.RootDirectory, "background.png")}";
        }

        //特殊
        public static class Caches
        {
            public static List<JsonGameInfo.Root> GameList = new List<JsonGameInfo.Root>();//游戏列表
            public static List<JsonTrainerInfo.Root> TrainerList = new List<JsonTrainerInfo.Root>();//修改器
            public static JsonDownloadIndex.Root? DownloadIndex = null;//下载索引

            public static BitmapImage LauncherBackground = null!;//背景图像
        }

        //字符串
        public static class Urls
        {
            public static string ServiceRootUrl = ServiceRootUrls.Gitee;//服务根Url
            public static readonly string DownloadIndexUrl = $"{ServiceRootUrl}/game-library/index.json";//下载索引
            public static readonly string UpdateIndexUrl = $"{ServiceRootUrl}/update/latest.json";//更新索引
            public static readonly string NoticeIndexUrl = $"{ServiceRootUrl}/notice/index.json";//公告索引
            public static readonly string FileIndexUrl = $"{ServiceRootUrl}/files/index.json";//文件索引

            public static class ServiceRootUrls
            {
                public static readonly string Gitee = "https://gitee.com/huamouren110/PvzLauncher.Service/raw/main";
                public static readonly string GitCode = "https://raw.gitcode.com/HuaMouRen/PvzLauncher.Service/raw/main";
            }
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
