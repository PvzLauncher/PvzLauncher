using PvzLauncherRemake.Utils.Game;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using static PvzLauncherRemake.Utils.UI.LocalizeService;

namespace PvzLauncherRemake.Controls
{
    /// <summary>
    /// UserCard.xaml 的交互逻辑
    /// </summary>
    public partial class UserCard : UserControl
    {


        public string Title { get; set; } = "Title";
        public string Description { get; set; }
        public GameIcons Icon { get; set; } = GameIcons.Origin;
        public string Version { get; set; }
        public string Size { get; set; }
        public string SupportVersion { get; set; }
        public bool isRecommend { get; set; }
        public bool isNew { get; set; }
        public bool isActive { get; set; }
        public bool isVirtual { get; set; }
        public object AttachedProperty { get; set; }
        public bool BigIconMode { get; set; } = false;
        public bool IsReadOnly { get; set; } = false;
        public bool IsFavorite { get; set; } = false;

        public UIElement? CustomControl { get; set; }



        private void PlayBorderAnimation(double from, double to, TimeSpan duration)
        {
            if (IsReadOnly)
                return;

            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = duration,
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            border.BeginAnimation(OpacityProperty, null);
            border.BeginAnimation(OpacityProperty, animation);
        }
        private void PlayMainAreaAniamtion(double from, double to, TimeSpan duration)
        {
            if (IsReadOnly)
                return;

            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = duration,
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };

            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }


        public UserCard()
        {
            InitializeComponent();
            Loaded += ((s, e) =>
            {
                if (BigIconMode)
                {
                    this.Height = 100;
                    grid_Icon.Height = 80; grid_Icon.Width = 80;
                    stackPanel_Title.Margin = new Thickness(90, 5, 5, 5);
                    stackPanel_Labels.Margin = new Thickness(90, 5, 5, 5);
                }


                if (IsReadOnly)
                {
                    border.Opacity = 1;
                }

                textBlock_Title.Text = Title;
                textBlock_Description.Text = Description;
                rect_favoriteBack.Visibility = IsFavorite ? Visibility.Visible : Visibility.Hidden;

                //自定义控件
                grid_customControl.Children.Clear();
                if (CustomControl != null)
                    grid_customControl.Children.Add(CustomControl);

                //图标
                var icon = GameIconConverter.ParseGameIconToUserControl(Icon);
                grid_Icon.Children.Clear();
                grid_Icon.Children.Add(icon);

                SetLabels();
            });


            rectangle_MouseTrigger.MouseEnter += (s, e) => PlayBorderAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            rectangle_MouseTrigger.MouseLeave += (s, e) => PlayBorderAnimation(1, 0, TimeSpan.FromMilliseconds(500));
            rectangle_MouseTrigger.MouseDown += (s, e) => PlayMainAreaAniamtion(1, 0.98, TimeSpan.FromMilliseconds(200));
            rectangle_MouseTrigger.MouseUp += (s, e) => PlayMainAreaAniamtion(0.98, 1, TimeSpan.FromMilliseconds(500));
        }

        public void SetLabels()
        {
            //清除
            stackPanel_Labels.Children.Clear();
            if (!string.IsNullOrEmpty(Version))
                AddLabel(Version, Color.FromArgb(204, 150, 150, 150), false);
            if (!string.IsNullOrEmpty(Size))
                AddLabel($"{Size} MB", Color.FromArgb(204, 0, 150, 150), false);
            if (!string.IsNullOrEmpty(SupportVersion))
                AddLabel($"{GetLoc("I18N.UserCard", "SupportVersion")}: {SupportVersion}", Color.FromArgb(204, 0, 0, 255), false);
            if (isRecommend)
                AddLabel($"{GetLoc("I18N.UserCard", "Recommend")}", Color.FromArgb(204, 0, 255, 0), true);
            if (isNew)
                AddLabel($"{GetLoc("I18N.UserCard", "New")}", Color.FromArgb(204, 100, 0, 255), true);
            if (isVirtual)
                AddLabel($"{GetLoc("I18N.UserCard", "Virtual")}", Color.FromArgb(204, 200, 120, 255), true);
            if (isActive)
                AddLabel($"{GetLoc("I18N.UserCard", "Active")}", Color.FromArgb(204, 255, 0, 0), true);
        }

        public void AddLabel(string content, Color color, bool textBold)
        {
            var label = new Grid
            {
                Margin = new Thickness(0, 0, 5, 0),
                Children =
                {
                    new Rectangle
                    {
                        Height=20,
                        RadiusX=3,
                        RadiusY=3,
                        Fill=new SolidColorBrush(color)
                    },
                    new TextBlock
                    {
                        Text=content,
                        HorizontalAlignment=HorizontalAlignment.Center,
                        VerticalAlignment=VerticalAlignment.Center,
                        Foreground=new SolidColorBrush(Colors.White),
                        Margin=new Thickness(5,0,5,0)
                    }
                }
            };
            stackPanel_Labels.Children.Add(label);
        }
    }
}
