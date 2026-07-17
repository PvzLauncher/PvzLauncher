using PvzLauncherRemake.Windows;
using System.Windows;

namespace PvzLauncherRemake.Utils.UI
{
    public enum NavigaionPages
    {
        Launch,
        Manage,
        Download,
        Task,
        Settings,
        About
    }

    public static class NavigationController
    {
        public static void Navigate(NavigaionPages target)
        {
            if (Application.Current.MainWindow is not WindowMain window)
                return;

            var navView = window.navView;


            switch (target)
            {
                case NavigaionPages.Launch:
                    navView.SelectedItem = window.navViewItem_Launch; break;
                case NavigaionPages.Manage:
                    navView.SelectedItem = window.navViewItem_Manage; break;
                case NavigaionPages.Download:
                    navView.SelectedItem = window.navViewItem_Download; break;

                case NavigaionPages.Task:
                    navView.SelectedItem = window.navViewItem_Task; break;
                case NavigaionPages.Settings:
                    navView.SelectedItem = window.navViewItem_Settings; break;
                case NavigaionPages.About:
                    navView.SelectedItem = window.navViewItem_About; break;
            }
        }
    }
}