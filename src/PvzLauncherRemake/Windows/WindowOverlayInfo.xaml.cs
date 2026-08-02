using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Utils.Services;
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

namespace PvzLauncherRemake.Windows
{
    /// <summary>
    /// WindowOverlayInfo.xaml 的交互逻辑
    /// </summary>
    public partial class WindowOverlayInfo : Window
    {
        public WindowOverlayInfo()
        {
            InitializeComponent();

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            Loaded += (s, e) =>
            {
                timer.Start();

                //设置窗口样式
                var hwnd = new WindowInteropHelper(this).Handle;
                var style = Win32APIHelper.GetWindowLongPtr(hwnd, Win32APIHelper.GWL_EXSTYLE);

                style |= Win32APIHelper.WS_EX_TRANSPARENT;//鼠标穿透
                style |= Win32APIHelper.WS_EX_TOOLWINDOW;//无AltTab
                style |= Win32APIHelper.WS_EX_NOACTIVATE;//无焦点

                Win32APIHelper.SetWindowLongPtr(hwnd, Win32APIHelper.GWL_EXSTYLE, new IntPtr(style));

            };

            Closing += (s, e) =>
            {
                timer.Stop(); timer = null;
            };

            timer.Tick += (s, e) =>
            {
                grid_logo.Visibility = Globals.Config.OverLayWindowSettings.InfoOverlay.ShowLogo ? Visibility.Visible : Visibility.Hidden;
            };


            


        }
    }
}
