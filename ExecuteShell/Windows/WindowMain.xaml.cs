using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ExecuteShell.Windows
{
    /// <summary>
    /// WindowMain.xaml 的交互逻辑
    /// </summary>
    public partial class WindowMain : Window
    {
        private void ExecuteMainProgram()
        {
            try
            {
                string executePath = AppDomain.CurrentDomain.BaseDirectory;
                string binDirctory = Path.Combine(executePath, "bin");
                string mainProgramPath = Path.Combine(binDirctory, "PvzLauncherRemake.exe");

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


        public WindowMain()
        {
            InitializeComponent();
            Loaded += (async (s, e) =>
            {
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
