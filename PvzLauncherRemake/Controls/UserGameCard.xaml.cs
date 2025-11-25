using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace PvzLauncherRemake.Controls
{
    /// <summary>
    /// UserGameCard.xaml 的交互逻辑
    /// </summary>
    public partial class UserGameCard : UserControl
    {
        public string Icon { get; set; } = "origin";
        public string Title { get; set; } = "Title";
        public string Version { get; set; } = "1.0.0.0";
        public bool isCurrent { get; set; } = false;


        public UserGameCard()
        {
            InitializeComponent();

            Loaded += LoadUI;
        }

        private void LoadUI(object sender, RoutedEventArgs e)
        {
            //图标
            List<Viewbox> icons = new List<Viewbox> { viewBox_beta, viewBox_origin };
            foreach (var icon in icons)
            {
                if (icon.Tag.ToString() != Icon)
                    icon.Visibility = Visibility.Hidden;
            }

            textBlock_Title.Text = Title;

            SetLabels();
        }

        public void SetLabels()
        {
            //添加标签
            stackPanel.Children.Clear();
            if (!string.IsNullOrEmpty(Version))
            {
                string xaml =
                    "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        "<Rectangle Fill=\"#7F000000\" RadiusX=\"3\" RadiusY=\"3\"/>" +
                        $"<TextBlock Text=\"{Version}\" FontSize=\"12\" Margin=\"2,2,2,2\" Foreground=\"White\"/>" +
                    "</Grid>";
                stackPanel.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
            if (isCurrent)
            {
                string xaml =
                    "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        "<Rectangle Fill=\"#CCFF0000\" RadiusX=\"3\" RadiusY=\"3\"/>" +
                        $"<TextBlock Text=\"活动\" FontSize=\"12\" Margin=\"2,2,2,2\" Foreground=\"White\" FontWeight=\"Bold\"/>" +
                    "</Grid>";
                stackPanel.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
        }
    }
}
