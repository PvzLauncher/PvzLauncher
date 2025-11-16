using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PvzLauncherRemake.Utils
{
    public static class NavigationController
    {
        public static void Navigate(DependencyObject obj, string target)
        {
            if (Window.GetWindow(obj) is MainWindow window)
                if (window.FindName("navView") is NavigationView navView)
                    if (navView.FindName($"navViewItem_{target}") is NavigationViewItem navViewItem)
                        navView.SelectedItem = navViewItem;
                    else
                        throw new Exception("找不到目标项");
                else
                    throw new Exception("找不到目标项");
            else
                throw new Exception("找不到目标项");
        }
    }
}
