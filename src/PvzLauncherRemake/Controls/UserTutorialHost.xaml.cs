using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PvzLauncherRemake.Controls
{
    /// <summary>
    /// UserTutorialHost.xaml 的交互逻辑
    /// </summary>
    public partial class UserTutorialHost : UserControl
    {
        //动画时长
        public Duration AnimationDuration { get; set; } = new Duration(TimeSpan.FromMilliseconds(1000));

        private Rect _hole = new Rect();

        public UserTutorialHost()
        {
            InitializeComponent();

            SizeChanged += (s, e) => UpdateMask(false);
        }

        /// <summary>
        /// 设置洞口位置
        /// </summary>
        /// <param name="target">矩形</param>
        public void SetHole(Rect target)
        {
            _hole = target;
            UpdateMask(true);
        }

        /// <summary>
        /// 设置洞口位置
        /// </summary>
        /// <param name="target">目标控件</param>
        /// <param name="offset">洞口大小偏移</param>
        public void SetHole(UIElement target, double offset = 5)
        {
            Point position = target.TranslatePoint(new Point(0, 0), this);

            SetHole(new Rect(position.X - offset, position.Y - offset, target.RenderSize.Width + 2 * offset, target.RenderSize.Height + 2 * offset));
        }

        private async void UpdateMask(bool animate)
        {
            if (ActualWidth <= 0 || ActualHeight <= 0)
                return;

            geometry_mask.Rect = new Rect(0, 0, ActualWidth, ActualHeight);

            if (!animate)
            {
                geometry_hole.BeginAnimation(RectangleGeometry.RectProperty, null);
                geometry_hole.Rect = _hole;
                return;
            }

            SetCardAnimate(false);
            geometry_hole.BeginAnimation(RectangleGeometry.RectProperty, new RectAnimation
            {
                To = _hole,
                Duration = AnimationDuration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            });
            await Task.Delay((int)(AnimationDuration.TimeSpan.TotalMilliseconds / 3 * 2));
            SetCardAnimate(true);
        }

        private void SetCardAnimate(bool state)
        {
            grid_tuCard.IsEnabled = state;

            var animate = new DoubleAnimation
            {
                From = state ? 0 : 1,
                To = state ? 1 : 0,
                Duration = TimeSpan.FromMilliseconds(AnimationDuration.TimeSpan.TotalMilliseconds / 3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            grid_tuCard.BeginAnimation(OpacityProperty, null);
            grid_tuCard.BeginAnimation(OpacityProperty, animate);
        }

    }
}
