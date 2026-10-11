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
        public event RoutedEventHandler? NextButtonClick;
        public event RoutedEventHandler? ExitButtonClick;

        public string Title { get; set; } = "Title";
        public string Text { get; set; } = "Content";

        private Rect _hole = new Rect();

        public UserTutorialHost()
        {
            InitializeComponent();

            SizeChanged += (s, e) =>
            {
                UpdateMask(false);
                UpdateCardPosition();
            };
            button_nextTutorial.Click += (s, e) => NextButtonClick?.Invoke(this, null);
            button_exitTutorial.Click += (s, e) => ExitButtonClick?.Invoke(this, null);
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

        /// <summary>
        /// 设置教程是否显示
        /// </summary>
        /// <param name="visible"></param>
        public void SetVisible(bool visible) => SetTutorialVisible(visible);




        //====

        private async void UpdateMask(bool animate)
        {
            if (this.ActualWidth <= 0 || this.ActualHeight <= 0)
                return;

            geometry_mask.Rect = new Rect(0, 0, ActualWidth, ActualHeight);

            if (!animate)
            {
                geometry_hole.BeginAnimation(RectangleGeometry.RectProperty, null);
                geometry_hole.Rect = _hole;
                UpdateCardPosition();
                return;
            }

            SetCardAnimate(false);

            geometry_hole.BeginAnimation(RectangleGeometry.RectProperty, new RectAnimation
            {
                To = _hole,
                Duration = AnimationDuration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            });

            await Task.Delay((int)(AnimationDuration.TimeSpan.TotalMilliseconds / 3));

            UpdateCardPosition();
            UpdateCardContent();

            await Task.Delay((int)(AnimationDuration.TimeSpan.TotalMilliseconds / 3));

            SetCardAnimate(true);
        }

        private void UpdateCardContent()
        {
            textBlock_cardTitle.Text = Title;
            textBlock_cardContent.Text = Text;
        }

        private void UpdateCardPosition()
        {
            if (grid_tuCard.ActualWidth <= 0 || grid_tuCard.ActualHeight <= 0)
                return;

            double spacing = 20;
            double cardWidth = grid_tuCard.ActualWidth;
            double cardHeight = grid_tuCard.ActualHeight;

            double maxX = Math.Max(0, ActualWidth - cardWidth);
            double maxY = Math.Max(0, ActualHeight - cardHeight);

            Rect bottom = new Rect(_hole.X + (_hole.Width - cardWidth) / 2, _hole.Bottom + spacing, cardWidth, cardHeight);
            Rect top = new Rect(_hole.X + (_hole.Width - cardWidth) / 2, _hole.Y - cardHeight - spacing, cardWidth, cardHeight);
            Rect right = new Rect(_hole.Right + spacing, _hole.Y + (_hole.Height - cardHeight) / 2, cardWidth, cardHeight);
            Rect left = new Rect(_hole.X - cardWidth - spacing, _hole.Y + (_hole.Height - cardHeight) / 2, cardWidth, cardHeight);

            Rect[] candidates = { bottom, top, right, left };

            foreach (Rect candidate in candidates)
            {
                if (candidate.Left >= 0 && candidate.Top >= 0 && candidate.Right <= ActualWidth && candidate.Bottom <= ActualHeight)
                {
                    SetCardPosition(candidate);
                    return;
                }
            }

            Rect best = candidates[0];
            double bestArea = GetVisibleArea(best);

            foreach (Rect candidate in candidates)
            {
                double area = GetVisibleArea(candidate);

                if (area > bestArea)
                {
                    best = candidate;
                    bestArea = area;
                }
            }

            best.X = Math.Max(0, Math.Min(best.X, maxX));
            best.Y = Math.Max(0, Math.Min(best.Y, maxY));

            SetCardPosition(best);
        }

        private double GetVisibleArea(Rect rect)
        {
            Rect visible = Rect.Intersect(rect, new Rect(0, 0, ActualWidth, ActualHeight));

            if (visible.IsEmpty)
                return 0;

            return visible.Width * visible.Height;
        }

        private void SetCardPosition(Rect rect)
        {
            grid_tuCard.BeginAnimation(Canvas.LeftProperty, null);
            grid_tuCard.BeginAnimation(Canvas.TopProperty, null);

            Canvas.SetLeft(grid_tuCard, rect.X);
            Canvas.SetTop(grid_tuCard, rect.Y);
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

        private void SetTutorialVisible(bool state)
        {
            this.IsEnabled = state;
            this.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
