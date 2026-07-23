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

            var selectItem = (UIElement)((TabControl)sender).SelectedContent;

            var tt = new TranslateTransform { Y = 25 };
            selectItem.RenderTransform = tt;

            tt.BeginAnimation(TranslateTransform.YProperty, null);
            selectItem.BeginAnimation(UIElement.OpacityProperty, null);

            selectItem.Opacity = 0;

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
            selectItem.BeginAnimation(UIElement.OpacityProperty, opacAnim);
        }
    }
}
