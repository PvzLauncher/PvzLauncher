using ModernWpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Utils.Configuration;
using PvzLauncherRemake.Utils.Services;
using System.Buffers;
using System.IO;
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
        #region init
        private async void Initialize()
        {
            //绑定事件
            Application.Current.DispatcherUnhandledException += DispatcherUnhandledExceptionHandler;
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            TaskScheduler.UnobservedTaskException += UnobservedTaskExceptionHandler ;



            //初始化配置文件
            if (!File.Exists(System.IO.Path.Combine(AppGlobals.ExecuteDirectory, "config.json")))
            {
                ConfigManager.CreateDefaultConfig();
            }
            //游戏目录
            if (!Directory.Exists(AppGlobals.GameDirectory))
            {
                Directory.CreateDirectory(AppGlobals.GameDirectory);
            }
            //修改器目录
            if (!Directory.Exists(AppGlobals.TrainerDirectory))
            {
                Directory.CreateDirectory(AppGlobals.TrainerDirectory);
            }

            //读配置
            ConfigManager.LoadConfig();

            //切换语言
            LocalizeManager.SwitchLanguage(AppGlobals.Config.LauncherConfig.Language);

            //加载列表
            await GameManager.LoadGameListAsync();
            await GameManager.LoadTrainerListAsync();
        }

        private async void InitializeLoaded()
        {
            ThemeManager.AddActualThemeChangedHandler(this.MainWindow, OnThemeChanged);

            //主题
            switch (AppGlobals.Config.LauncherConfig.Theme)
            {
                case "Light":
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light; OnThemeChanged(null!, null!); break;
                case "Dark":
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark; break;
            }
        }
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Initialize();

            var mainWindow = new WindowMain();
            this.MainWindow = mainWindow;

            InitializeLoaded();

            mainWindow.Show();
            mainWindow.Activate();
            mainWindow.Focus();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            AppLogger.logger.Warn($"[应用程序] 应用程序{(e.ApplicationExitCode != 0 ? "非" : null)}正常退出(ExitCode: {e.ApplicationExitCode})");
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
                Color = (Color)ColorConverter.ConvertFromString($"#0C{new string(colorFill, 6)}")
            };

        }

        #region 错误捕获

        private void DispatcherUnhandledExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        => ProcessUnhandledException(e.Exception);

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        => ProcessUnhandledException((Exception)e.ExceptionObject);

        private void UnobservedTaskExceptionHandler(object? sender, UnobservedTaskExceptionEventArgs e)
        => ProcessUnhandledException(e.Exception);

        private void ProcessUnhandledException(Exception ex)
        {
            MessageBox.Show($"发生了未捕获的错误\n\n\n{ex}\n\n\n程序即将退出...", "PvzLauncher", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}
