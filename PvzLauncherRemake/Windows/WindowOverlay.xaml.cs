using ModernWpf;
using NHotkey.Wpf;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Utils.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace PvzLauncherRemake.Windows
{
    /// <summary>
    /// WindowOverlay.xaml 的交互逻辑
    /// </summary>
    public partial class WindowOverlay : Window
    {
        private DispatcherTimer? _timer;
        private bool IsOverlayVisible = true;
        private WindowInteropHelper windowInteropHelper;

        public WindowOverlay()
        {
            InitializeComponent();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            //如游戏退出或进程信息为空则关闭覆盖界面
            if (!GameManager.IsGameRuning || AppProcess.Process == null || AppProcess.Process.HasExited)
                this.Close();

            AppProcess.Process!.Refresh();


            //同步窗口位置
            var result = Win32APIHelper.GetWindowArea(AppProcess.Process!.MainWindowHandle);

            this.Left = result.Left;
            this.Top = result.Top;
            this.Width = result.Width;
            this.Height = result.Height;


            //判断是否失焦
            var activeWindow = Win32APIHelper.GetActiveWindowHandle();
            if (activeWindow != AppProcess.Process.MainWindowHandle && activeWindow != windowInteropHelper.Handle)
                ToggleOverlay(false);

            //更新时间
            var now = DateTimeOffset.Now;

            textBlock_Time.Text = $"{now.ToString("HH:mm:ss")}";
            textBlock_Date.Text = $"{now.ToString("yyyy.MM.dd dddd")}";

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _timer?.Start();
            ToggleOverlay(false);

            ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);

            windowInteropHelper = new WindowInteropHelper(this);

            HotkeyManager.Current.AddOrReplace("ToggleOverlay", Key.P, ModifierKeys.Control | ModifierKeys.Alt, ((s, e) => ToggleOverlay()));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer?.Stop();
            _timer = null;
            HotkeyManager.Current.Remove("ToggleOverlay");
        }

        private void ToggleOverlay(bool? targetState = null)
        {
            if (targetState == null)
                IsOverlayVisible = !IsOverlayVisible;
            else
                IsOverlayVisible = (bool)targetState;

            if (IsOverlayVisible)
            {
                this.Visibility = Visibility.Visible;
            }
            else
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        private void button_HideOverlay_Click(object sender, RoutedEventArgs e) => ToggleOverlay(false);
    }
}
