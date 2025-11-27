using ModernWpf.Controls;
using System.Windows;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class NavigationController
    {
        public static void Navigate(DependencyObject obj, string target)
        {
            if (Window.GetWindow(obj) is MainWindow window)
                if (window.FindName("navView") is NavigationView navView)
                    if (navView.FindName($"navViewItem_{target}") is NavigationViewItem navViewItem)
                    {
                        logger.Info($"[导航视图控制器] 导航到 \"{target}\"");
                        navView.SelectedItem = navViewItem;
                    }
                    else
                        throw new Exception("找不到目标项");
                else
                    throw new Exception("找不到目标项");
            else
                throw new Exception("找不到目标项");
        }
    }
}
