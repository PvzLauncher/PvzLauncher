using ModernWpf.Controls;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils.Game;
using PvzLauncherRemake.Utils.UI;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static PvzLauncherRemake.Utils.UI.LocalizeService;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageDownloadConfirm.xaml 的交互逻辑
    /// </summary>
    public partial class PageDownloadDetail : ModernWpf.Controls.Page
    {
        public JsonDownloadIndex.GameInfo Info { get; set; }
        public string BaseDirectory { get; set; }
        public bool IsTrainer { get; set; }

        private string ScreeshotRootUrl = $"{Globals.Urls.ServiceRootUrl}/game-library/screenshots";

        private void StartImageAnimation(Image image)
        {
            //动画
            var thicknessAnimation = new ThicknessAnimation
            {
                From = new Thickness(-50, 0, 0, 0),
                To = new Thickness(0),
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            var doubleAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            image.BeginAnimation(MarginProperty, null);
            image.BeginAnimation(OpacityProperty, null);
            image.BeginAnimation(MarginProperty, thicknessAnimation);
            image.BeginAnimation(OpacityProperty, doubleAnimation);
        }


        public PageDownloadDetail()
        {
            InitializeComponent();
            Loaded += (async (s, e) =>
            {
                try
                {
                    //卡片
                    userCard.Title = Info!.Name;
                    userCard.Icon = GameIconConverter.ParseStringToGameIcons(Info.Icon);
                    userCard.Version = Info.Version;
                    userCard.Size = $"{Info.Size}";
                    userCard.isNew = Info.IsNew;
                    userCard.isRecommend = Info.IsRecommend;

                    if (Info is JsonDownloadIndex.TrainerInfo ti)
                        userCard.SupportVersion = ti.SupportVersion;


                    //简介
                    textBlock_Description.Text = "";
                    foreach (var line in Info.Descriptions)
                        textBlock_Description.Text = $"{textBlock_Description.Text}{line}\n";

                    //信息
                    stackPanel_Author.Children.Clear();
                    foreach (var author in Info.Authors)
                    {
                        var button = new HyperlinkButton
                        {
                            Content = author.Key,
                            MinWidth = 100,
                            IsEnabled = !string.IsNullOrEmpty(author.Value)
                        };
                        if (!string.IsNullOrEmpty(author.Value))
                            button.NavigateUri = new Uri(author.Value);
                        stackPanel_Author.Children.Add(button);
                    }
                    //下载按钮
                    button_Link.Visibility = (Info.LinkUrls == null || Info.LinkUrls?.Count < 1) ? Visibility.Collapsed : Visibility.Visible;
                    button_Download.Visibility = string.IsNullOrEmpty(Info.ShareUrl) ? Visibility.Collapsed : Visibility.Visible;
                    button_Manual.Visibility = string.IsNullOrEmpty(Info.ShareUrl) ? Visibility.Collapsed : Visibility.Visible;

                    stackPanel_Screenshot.Children.Clear();
                    using (var client = new HttpClient())
                    {
                        for (int i = 0; i < Info.Screenshot; i++)
                        {
                            string url = $"{ScreeshotRootUrl}/{Info.Name}/{i + 1}.png";

                            byte[] imageBytes = await client.GetByteArrayAsync(url);

                            using (var memoryStream = new MemoryStream(imageBytes))
                            {
                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.StreamSource = memoryStream;
                                bitmap.EndInit();
                                bitmap.Freeze();

                                var image = new Image
                                {
                                    MaxHeight = 250,
                                    Stretch = Stretch.Uniform,
                                    Source = bitmap,
                                    RenderTransformOrigin = new Point(0.5, 0.5)
                                };
                                image.RenderTransform = new ScaleTransform();
                                image.MouseEnter += ((s, e) => ImageMouseEnter(s));
                                image.MouseLeave += ((s, e) => ImageMouseLeave(s));
                                image.MouseUp += ImagePreview;

                                stackPanel_Screenshot.Children.Add(image);

                                StartImageAnimation(image);

                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    stackPanel_Screenshot.Children.Add(new TextBlock
                    {
                        Text = $"无法获取图像文件: {ex}"
                    });
                }
            });
        }

        private async void button_Link_Click(object sender, RoutedEventArgs e)
        {
            if (Info.LinkUrls.Count == 1)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Info.LinkUrls.First().Value,
                    UseShellExecute = true
                });
                return;
            }

            var stackPanel = new StackPanel();
            string targetUrl = Info.LinkUrls.First().Value;
            foreach (var link in Info.LinkUrls)
            {
                var radio = new RadioButton
                {
                    Content = link.Key,
                    Tag = link.Value,
                    IsChecked = stackPanel.Children.Count <= 0//默认选中第一个
                };
                stackPanel.Children.Add(radio);

                radio.Click += (s, e) => targetUrl = link.Value;
            }


            await DialogService.ShowDialogAsync(new ContentDialog
            {
                Title = "跳转官网",
                Content = stackPanel,
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            }, () =>
            {
                MessageBox.Show(targetUrl);
                Process.Start(new ProcessStartInfo
                {
                    FileName = targetUrl,
                    UseShellExecute = true
                });
            });

        }

        private async void button_Manual_Click(object sender, RoutedEventArgs e)
        {
            bool shouldOpenUrl = true;
            if (!string.IsNullOrEmpty(Info.SharePassword))
                await DialogService.ShowDialogAsync(new ContentDialog
                {
                    Content = new TextBlock
                    {
                        Text = $"密码: {Info.SharePassword}",
                        FontWeight = FontWeights.Bold,
                        FontSize = 20,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    PrimaryButtonText = "复制并前往",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                }, () => Clipboard.SetText(Info.SharePassword), null, () => shouldOpenUrl = false);

            if (shouldOpenUrl)
                Process.Start(new ProcessStartInfo
                {
                    FileName = Info.ShareUrl,
                    UseShellExecute = true
                });
        }

        private async void button_Download_Click(object sender, RoutedEventArgs e)
        {
            /*//确认下载
            bool confirm = false;
            await DialogService.ShowDialogAsync(new ContentDialog
            {
                Title = "下载确认",
                Content = $"是否下载 \"{Info.Name}\"",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            }, (() => confirm = true));
            if (!confirm) return;*/

            if (Info.Size >= 500)
            {
                bool isReturn = false;
                await DialogService.ShowDialogAsync(new ContentDialog
                {
                    Title = "警告",
                    Content = $"此游戏体积较大 ({Info.Size} MB) 启动器下载很可能掉速或者失败。建议手动前往浏览器下载",
                    PrimaryButtonText = "前往浏览器手动下载",
                    SecondaryButtonText = "仍然使用启动器下载",
                    DefaultButton = ContentDialogButton.Primary
                }, () =>
                {
                    button_Manual_Click(button_Manual, null!);
                    isReturn = true;
                });

                if (isReturn)
                    return;
            }





            //处理同名
            string? savePath = await GameManager.ResolveSameName(Info.Name, BaseDirectory);

            if (string.IsNullOrEmpty(savePath))
                return;

            //开始下载
            await GameManager.StartDownloadAsync(Info, savePath, IsTrainer);
        }

        private async void ImagePreview(Object sender, RoutedEventArgs e)
        {
            if (sender is not Image s)
                return;

            var dialog = new ContentDialog
            {
                Content = new UserScrollViewer
                {
                    Content = new Image { Source = s.Source, Stretch = Stretch.None },
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Visible
                },
                CloseButtonText = "关闭"
            };
            await DialogService.ShowDialogAsync(dialog);
        }

        private void ImageMouseEnter(object sender)
        {
            if (sender is not Image image || image.RenderTransform is not ScaleTransform st)
                return;

            Panel.SetZIndex(image, 100);

            var animation = new DoubleAnimation
            {
                From = 1,
                To = 1.1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            st.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            st.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            st.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            st.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        private void ImageMouseLeave(object sender)
        {
            if (sender is not Image image || image.RenderTransform is not ScaleTransform st)
                return;

            Panel.SetZIndex(image, 0);

            var animation = new DoubleAnimation
            {
                From = 1.1,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            st.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            st.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            st.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            st.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }
    }
}
