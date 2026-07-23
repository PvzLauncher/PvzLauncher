using PvzLauncherRemake.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PvzLauncherRemake.Utils.UI
{
    public static class TabControlAnimation
    {
        public static void TabControlAnimtion(object sender,RoutedEventArgs e)
        {
            if (e.OriginalSource != sender)
                return;

            var selectItem = ((TabControl)sender).SelectedContent;
            UIElement animControl = null!;

            if (selectItem is Grid)
                animControl = (Grid)selectItem;
            else if (selectItem is TabControl tabcontrol && tabcontrol.SelectedContent is Grid)
                animControl = (Grid)tabcontrol.SelectedContent;
            else if (selectItem is UserScrollViewer usv)
                animControl = (UserScrollViewer)selectItem;
            else
                return;

            var tt = new TranslateTransform { Y = 25 };
            animControl.RenderTransform = tt;

            tt.BeginAnimation(TranslateTransform.YProperty, null);
            animControl.BeginAnimation(UIElement.OpacityProperty, null);

            animControl.Opacity = 0;

            var margniAnim = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            var opacAnim = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            tt.BeginAnimation(TranslateTransform.YProperty, margniAnim);
            animControl.BeginAnimation(UIElement.OpacityProperty, opacAnim);
        }
    }
}
