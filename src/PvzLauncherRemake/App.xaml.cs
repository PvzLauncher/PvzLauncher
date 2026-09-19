using ModernWpf;
using PvzLauncherRemake.Utils.FileSystem;
using PvzLauncherRemake.Utils.Network;
using PvzLauncherRemake.Utils.UI;
using PvzLauncherRemake.Windows;
using Serilog;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace PvzLauncherRemake
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILogger logger = Log.ForContext<App>();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            #region 事件绑定
            Application.Current.DispatcherUnhandledException += DispatcherUnhandledExceptionHandler;
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            TaskScheduler.UnobservedTaskException += UnobservedTaskExceptionHandler;
            #endregion

            #region 单例检测
            var mutex = new Mutex(true, "pvzlauncher", out var createdNew);
            if (!createdNew)
            {
                Shutdown(101);
                return;
            }
            #endregion

            #region 日志初始化
            var logFileName = Path.Combine(Globals.Directories.LogDirectory, $"pvzl.latest.log");
            if (File.Exists(logFileName))
                File.Delete(logFileName);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u}] [{SourceContext}]: {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.File(
                    path: logFileName,
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u}] [{SourceContext}]: {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            //基本信息输出
            var sb = new StringBuilder();
            sb.AppendLine($"\n{new string('=', 10)}[基本系统信息]{new string('=', 10)}");
            sb.AppendLine($"操作系统: {Environment.OSVersion.VersionString}");
            sb.AppendLine($"系统架构: {RuntimeInformation.OSArchitecture}");
            sb.AppendLine($"Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture}");
            sb.AppendLine($"");
            sb.AppendLine($"CommandLine: {string.Join(' ', Environment.GetCommandLineArgs())}");
            sb.AppendLine($"DebugBuild? {Globals.Arguments.isDebugBuild}");
            sb.AppendLine($"CIBuild? {Globals.Arguments.isCIBuild}");
            sb.AppendLine($"");
            sb.AppendLine($"isUrl? {Globals.Arguments.isUrl}");
            sb.AppendLine($"isUpdate? {Globals.Arguments.isUpdate}");
            sb.Append(new string('=', 30));
            logger.Debug(sb.ToString());
            #endregion

            #region 参数处理

#if CI
            Globals.Arguments.isCIBuild = true;
#endif
#if DEBUG
            Globals.Arguments.isDebugBuild = true;
#endif

            string[] urlArgs;
            foreach (var arg in e.Args)
            {
                if (arg.StartsWith($"{Globals.Strings.ProtocolName}://"))
                {
                    urlArgs = arg.Split('/');
                    Globals.Arguments.isUrl = true;
                    break;
                }
            }
            //处理Url参数
            if (Globals.Arguments.isUrl)
            {
                //
            }
            else//处理普通参数
            {
                foreach (var arg in e.Args)
                {
                    switch (arg)
                    {
                        case "-update":
                            Globals.Arguments.isUpdate = true; break;
                    }
                }
            }
            #endregion

            #region 文件夹初始化                
            Directory.CreateDirectory(Globals.Directories.RootDirectory);//根目录
            Directory.CreateDirectory(Globals.Directories.GameDirectory);//游戏
            Directory.CreateDirectory(Globals.Directories.TrainerDirectory);//修改器
            Directory.CreateDirectory(Globals.Directories.LogDirectory);//日志
            #endregion

            #region 配置文件读取/初始化
            //初始化
            if (!File.Exists(Globals.Paths.ConfigPath))
                ConfigManager.CreateDefaultConfig();
            else
                //读配置
                ConfigManager.LoadConfig();

            //切换服务提供方
            switch (Globals.Config.Settings.LauncherConfig.ServiceProvider)
            {
                case "Gitee":
                    Globals.Urls.ServiceRootUrl = Globals.Urls.ServiceRootUrls.Gitee; break;
                case "GitCode":
                    Globals.Urls.ServiceRootUrl = Globals.Urls.ServiceRootUrls.GitCode; break;
                case "Github":
                    Globals.Urls.ServiceRootUrl = Globals.Urls.ServiceRootUrls.Github; break;
            }

            //切换语言
            LocalizeService.SwitchLanguage(Globals.Config.Settings.LauncherConfig.Language);
            #endregion

            #region URL协议注册
            UrlProtocolHelper.Register(Globals.Strings.ProtocolName, Globals.Paths.ExecutablePath);
            #endregion

            #region 主窗口创建
            var win = new WindowMain();
            this.MainWindow = win;
            Globals.Windows.WindowMain = win;
            win.Show();
            #endregion

            #region 窗口主题
            //主题
            ThemeManager.AddActualThemeChangedHandler(this.MainWindow, OnThemeChanged);
            switch (Globals.Config.Settings.LauncherConfig.Theme)
            {
                case "Light":
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light; break;
                case "Dark":
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark; break;
            }
            OnThemeChanged(null!, null!);
            #endregion
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (e.ApplicationExitCode == 101)//单例退出，不处理日志
                return;

            if (e.ApplicationExitCode == 0)
                logger.Information($"程序正常退出，退出码: {e.ApplicationExitCode}");
            else
                logger.Warning($"程序异常退出，退出码: {e.ApplicationExitCode}");

            //重命名日志
            Log.CloseAndFlush();
            var originalPath = Path.Combine(Globals.Directories.LogDirectory, "pvzl.latest.log");
            var newPath = Path.Combine(Globals.Directories.LogDirectory, $"pvzl.{Globals.StartupTime.ToUnixTimeMilliseconds()}.log");
            if (File.Exists(originalPath))
                File.Move(originalPath, newPath);
        }


        private void OnThemeChanged(object sender, EventArgs e)
        {
            var currentTheme = ThemeManager.GetActualTheme(this.MainWindow);

            if (currentTheme == ElementTheme.Light)
            {
                this.Resources["BorderBrush"] = new LinearGradientBrush
                {
                    EndPoint = new Point(0, 3),
                    MappingMode = BrushMappingMode.Absolute,
                    RelativeTransform = new ScaleTransform { CenterY = 0.5, ScaleY = -1 },
                    GradientStops =
                    {
                        new GradientStop{Color=(Color)ColorConverter.ConvertFromString($"#33000000"),Offset=0},
                        new GradientStop{Color=(Color)ColorConverter.ConvertFromString($"#19000000"),Offset=1},
                    }
                };
                this.Resources["BackgroundBrush"] = new SolidColorBrush
                {
                    Color = (Color)ColorConverter.ConvertFromString($"#02000000")
                };
                this.Resources["TUCardBackground"] = new SolidColorBrush { Color = Color.FromArgb(255, 243, 243, 243) };
                this.Resources["TUCardBorder"] = new SolidColorBrush { Color = Color.FromArgb(255, 223, 223, 223) };
            }
            else
            {
                this.Resources["BorderBrush"] = new LinearGradientBrush
                {
                    EndPoint = new Point(0, 3),
                    MappingMode = BrushMappingMode.Absolute,
                    RelativeTransform = new ScaleTransform { CenterY = 0.5, ScaleY = -1 },
                    GradientStops =
                    {
                        new GradientStop{Color=(Color)ColorConverter.ConvertFromString($"#33FFFFFF"),Offset=0},
                        new GradientStop{Color=(Color)ColorConverter.ConvertFromString($"#19FFFFFF"),Offset=1},
                    }
                };
                this.Resources["BackgroundBrush"] = new SolidColorBrush
                {
                    Color = (Color)ColorConverter.ConvertFromString($"#02FFFFFF")
                };
                this.Resources["TUCardBackground"] = new SolidColorBrush { Color = Color.FromArgb(255, 32, 32, 32) };
                this.Resources["TUCardBorder"] = new SolidColorBrush { Color = Color.FromArgb(255, 52, 52, 52) };
            }
        }




        private void DispatcherUnhandledExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e) =>
            ProcessUnhandledException(e.Exception);

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e) =>
            ProcessUnhandledException((Exception)e.ExceptionObject);

        private void UnobservedTaskExceptionHandler(object? sender, UnobservedTaskExceptionEventArgs e) =>
            ProcessUnhandledException(e.Exception);

        private void ProcessUnhandledException(Exception ex) =>
            ErrorReportDialog.Show(ex, true);
    }
}
