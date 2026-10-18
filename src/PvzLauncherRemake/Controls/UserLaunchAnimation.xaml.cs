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
    /// UserLaunchAnimation.xaml 的交互逻辑
    /// </summary>
    public partial class UserLaunchAnimation : UserControl
    {
        public UserLaunchAnimation()
        {
            InitializeComponent();
            Loaded += (s, e) => ResetAnimation();
        }

        public void ResetAnimation()
        {
            //初始化状态
            var elli1Trans = elli_ani_1.RenderTransform as ScaleTransform;
            var elli2Trans = elli_ani_2.RenderTransform as ScaleTransform;
            var elli3Trans = elli_ani_3.RenderTransform as ScaleTransform;
            var contentTrans = grid_ani_content.RenderTransform as ScaleTransform;

            elli1Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            elli1Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            elli2Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            elli2Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            elli3Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            elli3Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            contentTrans?.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            contentTrans?.BeginAnimation(ScaleTransform.ScaleYProperty, null);


            rect_ani_back.Opacity = 0;
            elli_ani_1.Opacity = 0; elli_ani_2.Opacity = 0; elli_ani_3.Opacity = 0;
            grid_ani_content.Opacity = 0;

            elli1Trans?.ScaleX = 1.5; elli1Trans?.ScaleY = 1.5;
            elli2Trans?.ScaleX = 2; elli2Trans?.ScaleY = 2;
            elli3Trans?.ScaleX = 2.5; elli3Trans?.ScaleY = 2.5;
            contentTrans?.ScaleX = 1.5; contentTrans?.ScaleY = 1.5;
        }

        public async void PlayAnimation()
        {
            var elli1Trans = elli_ani_1.RenderTransform as ScaleTransform;
            var elli2Trans = elli_ani_2.RenderTransform as ScaleTransform;
            var elli3Trans = elli_ani_3.RenderTransform as ScaleTransform;
            var contentTrans = grid_ani_content.RenderTransform as ScaleTransform;
            ResetAnimation();


            rect_ani_back.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                From = 0,
                To = 0.2,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            });
            var elliScaleAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            var elliOpacityAnimation = new DoubleAnimation
            {
                To = 0.2,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            elli1Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, elliScaleAnimation);
            elli1Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, elliScaleAnimation);

            elli2Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, elliScaleAnimation);
            elli2Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, elliScaleAnimation);

            elli3Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, elliScaleAnimation);
            elli3Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, elliScaleAnimation);


            elli_ani_1.BeginAnimation(OpacityProperty, elliOpacityAnimation);
            elli_ani_2.BeginAnimation(OpacityProperty, elliOpacityAnimation);
            elli_ani_3.BeginAnimation(OpacityProperty, elliOpacityAnimation);

            await Task.Delay(100);

            contentTrans?.BeginAnimation(ScaleTransform.ScaleXProperty, elliScaleAnimation);
            contentTrans?.BeginAnimation(ScaleTransform.ScaleYProperty, elliScaleAnimation);
            grid_ani_content.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            });
            //退出动画

            await Task.Delay(800);

            var opacityAniOut = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            var scaleAniOut = new DoubleAnimation
            {
                To = 3,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };

            rect_ani_back.BeginAnimation(OpacityProperty, opacityAniOut);
            elli_ani_1.BeginAnimation(OpacityProperty, opacityAniOut);
            elli_ani_2.BeginAnimation(OpacityProperty, opacityAniOut);
            elli_ani_3.BeginAnimation(OpacityProperty, opacityAniOut);
            elli1Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAniOut);
            elli1Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAniOut);
            elli2Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAniOut);
            elli2Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAniOut);
            elli3Trans?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAniOut);
            elli3Trans?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAniOut);

            grid_ani_content.BeginAnimation(OpacityProperty, opacityAniOut);
            contentTrans?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAniOut);
            contentTrans?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAniOut);

            return;
        }
    }
}
