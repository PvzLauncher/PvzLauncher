
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using PvzLauncherRemake.Utils.FileSystem;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Utils.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PvzLauncherRemake.Windows
{
    /// <summary>
    /// WindowOverlayInfo.xaml 的交互逻辑
    /// </summary>
    public partial class WindowOverlayInfo : Window
    {
        private readonly Grid slotKeyTemplte = new Grid
        {
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Ellipse
                {
                    Fill= new SolidColorBrush(Colors.White),
                    Stroke= new SolidColorBrush(Colors.Black),
                    Width=15,Height=15
                },
                new TextBlock
                {
                    Text="0",
                    HorizontalAlignment=HorizontalAlignment.Center,
                    VerticalAlignment=VerticalAlignment.Center
                }
            }
        };
        private List<Grid> slotKeyCtrls = new List<Grid>();

        private JsonGameInfo.Root GameInfo = JsonHelper.ReadJson<JsonGameInfo.Root>(System.IO.Path.Combine(Globals.Directories.GameDirectory, Globals.Config.CurrentGame, ".pvzl.json"));



        public WindowOverlayInfo()
        {
            InitializeComponent();

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
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

                //创建键位指示

                for (int i = 0; i < 11; i++)
                {
                    var ctrl = VisualTreeTools.CloneControl(slotKeyTemplte);
                    if (ctrl.Children[1] is not TextBlock tb)
                        throw new Exception("这个错误本来不应该抛出，但是你既然看到了这行文字就证明它被抛出了。这表明程序已经出现严重错误，请联系开发者解决问题");
                    tb.Text = i == 10 ? $"~" : $"{i + 1}";
                    slotKeyCtrls.Add(ctrl);
                }
                foreach (var ctrl in slotKeyCtrls)
                    grid_slot.Children.Add(ctrl);

            };

            Closing += (s, e) =>
            {
                timer.Stop(); timer = null;
            };

            timer.Tick += (s, e) =>
            {
                //重载游戏信息
                GameInfo = JsonHelper.ReadJson<JsonGameInfo.Root>(System.IO.Path.Combine(Globals.Directories.GameDirectory, Globals.Config.CurrentGame, ".pvzl.json"));

                grid_logo.Visibility = Globals.Config.OverLayWindowSettings.InfoOverlay.ShowLogo ? Visibility.Visible : Visibility.Hidden;
                grid_root.Opacity = Globals.Config.OverLayWindowSettings.InfoOverlay.Opacity;
                for (int i = 0; i < slotKeyCtrls.Count; i++)
                {
                    var pos = GameInfo.Config.SlotPositions[i == slotKeyCtrls.Count - 1 ? 0 : i + 1];
                    if (Globals.Config.OverLayWindowSettings.InfoOverlay.ShowSlotKey)
                    {
                        slotKeyCtrls[i].Margin = new Thickness(pos.X, pos.Y, 0, 0);
                        slotKeyCtrls[i].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        slotKeyCtrls[i].Visibility = Visibility.Hidden;
                    }

                }
            };





        }
    }
}
