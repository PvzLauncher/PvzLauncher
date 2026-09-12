using ModernWpf;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Utils.FileSystem;
using PvzLauncherRemake.Utils.Game;
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
        private static Mutex? _mutex;
        private const string mutexName = "PvzLauncher";
        private bool _isSingleShutdown = false;


        private void Initialize()
        {
            //初始化Logger
            var logFileName = Path.Combine(Globals.Directories.LogDirectory, $"pvzl.log.latest.log");
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
            var logger = Log.ForContext<App>();

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

            //是否CI构建
#if CI
            Globals.Arguments.isCIBuild = true;
#endif
            //是否Debug构建
#if DEBUG
            Globals.Arguments.isDebugBuild = true;
#endif

            //初始化根目录
            if (!Directory.Exists(Globals.Directories.RootDirectory))
                Directory.CreateDirectory(Globals.Directories.RootDirectory);
            //初始化配置文件
            if (!File.Exists(Globals.Paths.ConfigPath))
                ConfigManager.CreateDefaultConfig();
            //游戏目录
            if (!Directory.Exists(Globals.Directories.GameDirectory))
                Directory.CreateDirectory(Globals.Directories.GameDirectory);
            //修改器目录
            if (!Directory.Exists(Globals.Directories.TrainerDirectory))
                Directory.CreateDirectory(Globals.Directories.TrainerDirectory);
            //日志目录
            if (!Directory.Exists(Globals.Directories.LogDirectory))
                Directory.CreateDirectory(Globals.Directories.LogDirectory);

            //读配置
            ConfigManager.LoadConfig();

            //注册URL协议
            UrlProtocolHelper.Register(Globals.Strings.ProtocolName, Globals.Paths.ExecutablePath);

            //切换语言
            LocalizeService.SwitchLanguage(Globals.Config.Settings.LauncherConfig.Language);

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
        }

        private void HandleArgs(string[] args)
        {
            string[] urlArgs;
            foreach (var arg in args)
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
                foreach (var arg in args)
                {
                    switch (arg)
                    {
                        case "-update":
                            Globals.Arguments.isUpdate = true; break;
                    }
                }
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //绑定事件
            Application.Current.DispatcherUnhandledException += DispatcherUnhandledExceptionHandler;
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            TaskScheduler.UnobservedTaskException += UnobservedTaskExceptionHandler;

            //单例检测
            _mutex = new Mutex(true, mutexName, out var createdNew);
            if (!createdNew)
            {
                _isSingleShutdown = true;
                Shutdown(0);
                return;
            }

            //INIT
            HandleArgs(e.Args);
            Initialize();

            var mainWindow = new WindowMain();
            this.MainWindow = mainWindow;
            Globals.Windows.WindowMain = mainWindow;

            mainWindow.Show();

            //主题
            ThemeManager.AddActualThemeChangedHandler(this.MainWindow, OnThemeChanged);
            switch (Globals.Config.Settings.LauncherConfig.Theme)
            {
                case "Light":
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light; OnThemeChanged(null!, null!); break;
                case "Dark":
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark; break;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_isSingleShutdown)
                return;

            var logger = Log.ForContext<App>();

            logger.Information($"程序{(e.ApplicationExitCode == 0 ? "正常" : "异常")}退出，退出码: {e.ApplicationExitCode}");
            //重命名日志
            Log.CloseAndFlush();
            var originalPath = Path.Combine(Globals.Directories.LogDirectory, "pvzl.log.latest.log");
            var newPath = Path.Combine(Globals.Directories.LogDirectory, $"pvzl.log.{Globals.StartupTime.ToUnixTimeMilliseconds()}.log");
            if (File.Exists(originalPath))
                File.Move(originalPath, newPath);

            base.OnExit(e);
        }




        private void OnThemeChanged(object sender, EventArgs e)
        {
            var currentTheme = ThemeManager.GetActualTheme(this.MainWindow);


            char colorFill = '0';

            if (currentTheme == ElementTheme.Light)
                colorFill = '0';
            else if (currentTheme == ElementTheme.Dark)
                colorFill = 'F';

            this.Resources["BorderBrush"] = new LinearGradientBrush
            {
                EndPoint = new Point(0, 3),
                MappingMode = BrushMappingMode.Absolute,
                RelativeTransform = new ScaleTransform { CenterY = 0.5, ScaleY = -1 },
                GradientStops =
                {
                    new GradientStop{Color=(Color)ColorConverter.ConvertFromString($"#33{new string(colorFill,6)}"),Offset=0},
                    new GradientStop{Color=(Color)ColorConverter.ConvertFromString($"#19{new string(colorFill,6)}"),Offset=1},
                }
            };
            this.Resources["BackgroundBrush"] = new SolidColorBrush
            {
                Color = (Color)ColorConverter.ConvertFromString($"#02{new string(colorFill, 6)}")
            };

            if (currentTheme == ElementTheme.Light)
            {
                this.Resources["TUCardBackground"] = new SolidColorBrush { Color = Color.FromArgb(255, 243, 243, 243) };
                this.Resources["TUCardBorder"] = new SolidColorBrush { Color = Color.FromArgb(255, 223, 223, 223) };
            }
            else if (currentTheme == ElementTheme.Dark)
            {
                this.Resources["TUCardBackground"] = new SolidColorBrush { Color = Color.FromArgb(255, 32, 32, 32) };
                this.Resources["TUCardBorder"] = new SolidColorBrush { Color = Color.FromArgb(255, 52, 52, 52) };
            }
        }

        #region 错误捕获

        private void DispatcherUnhandledExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e) =>
            ProcessUnhandledException(e.Exception);

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e) =>
            ProcessUnhandledException((Exception)e.ExceptionObject);

        private void UnobservedTaskExceptionHandler(object? sender, UnobservedTaskExceptionEventArgs e) =>
            ProcessUnhandledException(e.Exception);

        private void ProcessUnhandledException(Exception ex) =>
            ErrorReportDialog.Show(ex, true);

        #endregion
    }
}
