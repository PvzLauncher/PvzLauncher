using H.Hooks;
using ModernWpf;
using ModernWpf.Controls;
using NHotkey.Wpf;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Utils.Configuration;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Utils.UI;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using WindowsInput;

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
        private LowLevelKeyboardHook _hook = new LowLevelKeyboardHook();
        private InputSimulator _inputSim = new InputSimulator();

        private int winLeft = 0;
        private int winTop = 0;

        private enum PageType
        {
            Main, PositionSelector
        }
        private void SwitchPage(PageType type)
        {
            grid_pageMain.Visibility = Visibility.Hidden;
            grid_pagePosSelection.Visibility = Visibility.Hidden;
            switch (type)
            {
                case PageType.Main: grid_pageMain.Visibility = Visibility.Visible; break;
                case PageType.PositionSelector: grid_pagePosSelection.Visibility = Visibility.Visible; break;
            }
        }









        public WindowOverlay()
        {
            InitializeComponent();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            _timer.Tick += Timer_Tick;







            //卡槽选择
            var slotKeys = new Dictionary<H.Hooks.Key, int>()
            {
                [H.Hooks.Key.D1] = 1,
                [H.Hooks.Key.D2] = 2,
                [H.Hooks.Key.D3] = 3,
                [H.Hooks.Key.D4] = 4,
                [H.Hooks.Key.D5] = 5,
                [H.Hooks.Key.D6] = 6,
                [H.Hooks.Key.D7] = 7,
                [H.Hooks.Key.D8] = 8,
                [H.Hooks.Key.D9] = 9,
                [H.Hooks.Key.D0] = 10,

                [H.Hooks.Key.Oem3] = 0,
            };
            _hook.Down += (s, e) =>
            {
                if (!slotKeys.ContainsKey(e.CurrentKey))
                    return;

                //不处于游戏窗口不触发
                if (Win32APIHelper.GetActiveWindowHandle() != GameManager.GameProcess.MainWindowHandle)
                    return;

                if (!Globals.Config.OverLayWindowSettings.SlotHotkeyEnabled)
                    return;

                var targetPos = Globals.Config.OverLayWindowSettings.SlotPositions[slotKeys[e.CurrentKey]];
                var targetPosFinal = new System.Drawing.Point(winLeft + targetPos.X, winTop + targetPos.Y);
                var currentPos = Win32APIHelper.GetCursorPos();
                if (currentPos.X == -1 || currentPos.Y == -1)
                    throw new Exception("无法获得鼠标指针坐标");

                Win32APIHelper.SetCursorPos(targetPosFinal);

                _inputSim.Mouse
                .LeftButtonDown()
                .LeftButtonUp();

                Win32APIHelper.SetCursorPos(new System.Drawing.Point(currentPos.X, currentPos.Y));
            };
            _hook.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            //如游戏退出或进程信息为空则关闭覆盖界面
            if (!GameManager.IsGameRuning || GameManager.GameProcess == null || GameManager.GameProcess.HasExited)
                this.Close();

            GameManager.GameProcess!.Refresh();


            //同步窗口位置
            var result = Win32APIHelper.GetWindowArea(GameManager.GameProcess!.MainWindowHandle);

            this.Left = result.Left;
            this.Top = result.Top;
            this.Width = result.Width;
            this.Height = result.Height;

            winLeft = (int)this.Left;
            winTop = (int)this.Top;


            //判断是否失焦
            var activeWindow = Win32APIHelper.GetActiveWindowHandle();
            if (activeWindow != GameManager.GameProcess.MainWindowHandle && activeWindow != windowInteropHelper.Handle)
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

            HotkeyManager.Current.AddOrReplace("ToggleOverlay", System.Windows.Input.Key.P, ModifierKeys.Control | ModifierKeys.Alt, ((s, e) => ToggleOverlay()));




            //加载配置
            toggleSwitch_slotHotkeyEnabled.IsOn = Globals.Config.OverLayWindowSettings.SlotHotkeyEnabled;
            button_setSlotPos.IsEnabled = toggleSwitch_slotHotkeyEnabled.IsOn;
            toggleSwitch_slotHotkeyEnabled.Toggled += (s, e) =>
            {
                button_setSlotPos.IsEnabled = toggleSwitch_slotHotkeyEnabled.IsOn;
                Globals.Config.OverLayWindowSettings.SlotHotkeyEnabled = toggleSwitch_slotHotkeyEnabled.IsOn;
                ConfigManager.SaveConfig();
            };


            ConfigManager.SaveConfig();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer?.Stop();
            _timer = null;
            HotkeyManager.Current.Remove("ToggleOverlay");

            _hook.Stop();
            _hook.Dispose();
        }

        private void ToggleOverlay(bool? targetState = null)
        {
            if (targetState == null)
                IsOverlayVisible = !IsOverlayVisible;
            else
                IsOverlayVisible = (bool)targetState;

            if (IsOverlayVisible)
            {
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }

        private void button_HideOverlay_Click(object sender, RoutedEventArgs e) => ToggleOverlay(false);

        private async void button_KillGame_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "结束游戏",
                Content = "确定结束游戏吗？",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            };
            await DialogManager.ShowDialogAsync(dialog, (async () => await GameManager.KillGame()), displayArea: DialogDisplayArea.Overlay);
        }

        private void button_setSlotPos_Click(object sender, RoutedEventArgs e)
        {
            SwitchPage(PageType.PositionSelector);
            currentPosSet = 1;
        }

        private int currentPosSet = 1;

        private void rect_posHit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var currentMousePos = Win32APIHelper.GetCursorPos();
            if (currentMousePos.X == -1 || currentMousePos.Y == -1)
                throw new Exception("无法获得鼠标指针坐标");
            Globals.Config.OverLayWindowSettings.SlotPositions[currentPosSet == 11 ? 0 : currentPosSet] = new System.Drawing.Point(currentMousePos.X - (int)this.Left, currentMousePos.Y - (int)this.Top);
            ConfigManager.SaveConfig();

            if (currentPosSet == 11)
            {
                SwitchPage(PageType.Main);
                return;
            }

            currentPosSet++;
        }
    }
}
