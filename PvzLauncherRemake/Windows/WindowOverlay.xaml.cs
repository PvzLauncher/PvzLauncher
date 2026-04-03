using ModernWpf;
using NHotkey;
using NHotkey.Wpf;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Utils.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static PvzLauncherRemake.Utils.Configuration.LocalizeManager;

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
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer?.Stop();
            _timer = null;
            HotkeyManager.Current.Remove("ToggleOverlay");
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            //注册热键
            try
            {
                //切换覆盖层显示
                HotkeyManager.Current.AddOrReplace("ToggleOverlay", Key.P, ModifierKeys.Control | ModifierKeys.Alt, ((s, e) => ToggleOverlay()));
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
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
