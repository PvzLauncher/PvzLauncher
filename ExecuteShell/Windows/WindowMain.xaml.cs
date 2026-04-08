using ExecuteShell.Utils;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ExecuteShell.Windows
{
    /// <summary>
    /// WindowMain.xaml 的交互逻辑
    /// </summary>
    public partial class WindowMain : Window
    {
        public static string executePath = AppDomain.CurrentDomain.BaseDirectory;
        public static string binDirctory = Path.Combine(executePath, "bin");
        public static string mainProgramPath = Path.Combine(binDirctory, "PvzLauncherRemake.exe");
        public static string updateServicePath = Path.Combine(binDirctory, "StdUpdateService.exe");

        private void ExecuteMainProgram()
        {
            try
            {
                if (!Path.Exists(mainProgramPath))
                    throw new FileNotFoundException("The main program executable file does not exist.", mainProgramPath);

                Process.Start(new ProcessStartInfo
                {
                    FileName = mainProgramPath,
                    Arguments = "-shell",
                    UseShellExecute = true,
                    WorkingDirectory = binDirctory
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception occurred when starting the main program.\n\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
            }
        }

        private async Task UnlockFile(string filePath)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"Unblock-File -Path '{filePath}'\"",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                await process?.WaitForExitAsync()!;
            }
            catch (Exception)
            {
                try
                {
                    //备选方案
                    string zonePath = filePath + ":Zone.Identifier";
                    if (File.Exists(zonePath))
                        File.Delete(zonePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法解锁文件 \"{filePath}\"，请尝试进入文件的属性，勾选\"解除锁定\"复选框并应用。随后再次尝试启动此程序\n\n详细信息: {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown(1);
                }
            }
        }

        public WindowMain()
        {
            InitializeComponent();
            Loaded += (async (s, e) =>
            {
                //设置穿透
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                if (hWnd != IntPtr.Zero)
                {
                    int extendedStyle = WinAPIHelper.GetWindowLong(hWnd, WinAPIHelper.GWL_EXSTYLE);

                    // 添加 WS_EX_LAYERED 和 WS_EX_TRANSPARENT 标志
                    // WS_EX_TRANSPARENT 使窗口对鼠标点击完全透明（穿透）
                    WinAPIHelper.SetWindowLong(hWnd, WinAPIHelper.GWL_EXSTYLE, extendedStyle | WinAPIHelper.WS_EX_LAYERED | WinAPIHelper.WS_EX_TRANSPARENT);
                }

                await UnlockFile(mainProgramPath);
                await UnlockFile(updateServicePath);


                var iconTrans = (ScaleTransform)icon.RenderTransform;
                iconTrans.ScaleX = 2.5;
                iconTrans.ScaleY = 2.5;

                var opacityAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(1000),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };
                var transformAnimation = new DoubleAnimation
                {
                    From = 2.5,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(1500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };
                var opacityOutAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(1000),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseIn }
                };
                var transformOutAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 0.2,
                    Duration = TimeSpan.FromMilliseconds(1500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseIn }
                };

                icon.BeginAnimation(OpacityProperty, opacityAnimation);
                iconTrans.BeginAnimation(ScaleTransform.ScaleXProperty, transformAnimation);
                iconTrans.BeginAnimation(ScaleTransform.ScaleYProperty, transformAnimation);

                ExecuteMainProgram();

                await Task.Delay(1500);
                icon.BeginAnimation(OpacityProperty, null);
                iconTrans.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                iconTrans.BeginAnimation(ScaleTransform.ScaleYProperty, null);

                icon.BeginAnimation(OpacityProperty, opacityOutAnimation);
                iconTrans.BeginAnimation(ScaleTransform.ScaleXProperty, transformOutAnimation);
                iconTrans.BeginAnimation(ScaleTransform.ScaleYProperty, transformOutAnimation);

                await Task.Delay(2000);
                Application.Current.Shutdown(0);
            });
        }
    }
}
