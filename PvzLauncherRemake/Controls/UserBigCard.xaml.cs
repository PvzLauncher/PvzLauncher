using PvzLauncherRemake.Controls.Icons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PvzLauncherRemake.Controls
{
    /// <summary>
    /// UserBigCard.xaml 的交互逻辑
    /// </summary>
    public partial class UserBigCard : UserControl
    {
        public string Title { get; set; } = "Title";
        public string Subtitle { get; set; } = "Subtitle";
        public UIElement Icon { get; set; } = new GameIconUnknown();


        public UserBigCard()
        {
            InitializeComponent();

            UpdateControl();
        }

        public void UpdateControl()
        {
            textblock_Title.Text = Title;
            textblock_Subtitle.Text = Subtitle;

            viewbox_Icon.Child = Icon;
        }

        private void rect_MouseTrigger_MouseEnter(object sender, MouseEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            border.BeginAnimation(OpacityProperty, null);
            border.BeginAnimation(OpacityProperty, animation);
        }

        private void rect_MouseTrigger_MouseLeave(object sender, MouseEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                To = 0,
                From = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            border.BeginAnimation(OpacityProperty, null);
            border.BeginAnimation(OpacityProperty, animation);
        }

        private void rect_MouseTrigger_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                To = 0.98,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };

            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        private void rect_MouseTrigger_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                To = 1,
                From = 0.98,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };

            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }
    }
}
