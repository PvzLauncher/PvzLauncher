using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PvzLauncherRemake.Controls
{
    /// <summary>
    /// UserTrainerCard.xaml 的交互逻辑
    /// </summary>
    public partial class UserTrainerCard : UserControl
    {
        public string Title { get; set; } = "Title";
        public Icon Icon { get; set; }
        public string Version { get; set; } = null!;
        public string SupportVersion { get; set; } = null!;
        public bool isCurrent { get; set; } = false;


        public UserTrainerCard()
        {
            InitializeComponent();
            Loaded += ((sender, e) =>
            {
                textBlock_Title.Text = Title;
                if (Icon != null)
                    image.Source = Imaging.CreateBitmapSourceFromHIcon(Icon!.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());


                SetLabels();
            });
        }

        public void SetLabels()
        {
            stackPanel_Labels.Children.Clear();
            if (!string.IsNullOrWhiteSpace(Version))
            {
                string xaml =
                    "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        "<Rectangle Fill=\"#FF8C8C8C\" RadiusX=\"3\" RadiusY=\"3\"/>" +
                        $"<TextBlock Text=\"{Version}\" Margin=\"2,2,2,2\" Foreground=\"White\"/>" +
                    "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
            if (!string.IsNullOrWhiteSpace(SupportVersion))
            {
                string xaml =
                    "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        "<Rectangle Fill=\"#FF3232FF\" RadiusX=\"3\" RadiusY=\"3\"/>" +
                        $"<TextBlock Text=\"{SupportVersion}\" Margin=\"2,2,2,2\" Foreground=\"White\"/>" +
                    "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
            if (isCurrent)
            {
                string xaml =
                    "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        "<Rectangle Fill=\"#FFFF3232\" RadiusX=\"3\" RadiusY=\"3\"/>" +
                        "<TextBlock Text=\"活动\" Margin=\"2,2,2,2\" Foreground=\"White\" FontWeight=\"Bold\"/> " +
                    "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
        }
    }
}
