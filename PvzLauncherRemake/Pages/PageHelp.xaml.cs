using HuaZi.Library.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageHelp.xaml 的交互逻辑
    /// </summary>
    public partial class PageHelp : ModernWpf.Controls.Page
    {
        private List<JsonHelpIndex.CardInfo[]> _historyCards = new List<JsonHelpIndex.CardInfo[]>();
        private List<JsonHelpIndex.ContentInfo[]?> _historyContents = new List<JsonHelpIndex.ContentInfo[]?>();


        #region Init
        public async void Initialize()
        {
            try
            {
                //获取Index
                if (AppGlobals.HelpIndex == null)
                {
                    using (var client = new HttpClient())
                        AppGlobals.HelpIndex = Json.ReadJson<JsonHelpIndex.Index>(await client.GetStringAsync(AppGlobals.HelpIndexUrl));
                }


                ChangePage(AppGlobals.HelpIndex.Root, null);
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }
        #endregion

        #region ChangePage
        public async void ChangePage(JsonHelpIndex.CardInfo[] cards, JsonHelpIndex.ContentInfo[]? contents, bool isBack = false)
        {
            try
            {
                if (!isBack)
                {
                    _historyCards.Add(cards);
                    _historyContents.Add(contents);
                }


                stackPanel.Children.Clear();

                if (_historyCards.Count > 1 && _historyContents.Count > 1) 
                {
                    var backCard = new UserBigCard
                    {
                        Title = "返回",
                        Subtitle = "返回上一层级",
                        ShowRightArrow = false
                    };
                    backCard.MouseUp += ((s, e) =>
                    {
                        _historyCards.RemoveAt(_historyCards.Count - 1);
                        _historyContents.RemoveAt(_historyContents.Count - 1);

                        ChangePage(_historyCards[^1], _historyContents[^1], true);
                    });
                    stackPanel.Children.Add(backCard);
                }


                



                foreach (var card in cards)
                {
                    var userBigCard = new UserBigCard
                    {
                        Title = card.Title,
                        Subtitle = card.Subtitle,
                        Icon = GameIconConverter.ParseGameIconToUserControl(GameIconConverter.ParseStringToGameIcons(card.Icon)),
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    userBigCard.MouseUp += ((s, e) => ChangePage(card.Childrens,card.Content));
                    stackPanel.Children.Add(userBigCard);
                    userBigCard.FadeIn();

                    await Task.Delay(100);
                }

                if (contents != null)
                {
                    var animationFadeIn = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(1000),
                        EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                    };


                    foreach (var content in contents)
                    {
                        if (content.Type == "text")
                        {
                            var textBlock = new TextBlock
                            {
                                Text = content.Content,
                                Margin = new Thickness(0, 0, 0, 10)
                            };
                            stackPanel.Children.Add(textBlock);

                            textBlock.BeginAnimation(OpacityProperty, null);
                            textBlock.BeginAnimation(OpacityProperty, animationFadeIn);
                        }
                        else if (content.Type == "image") 
                        {
                            using(var client=new HttpClient())
                            {
                                byte[] imageBytes = await client.GetByteArrayAsync(content.Content);
                                using(var memoryStream=new MemoryStream(imageBytes))
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
                                        Margin = new Thickness(0, 0, 0, 10)
                                    };

                                    stackPanel.Children.Add(image);
                                    image.BeginAnimation(OpacityProperty, null);
                                    image.BeginAnimation(OpacityProperty, animationFadeIn);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }
        #endregion


        public PageHelp()
        {
            InitializeComponent();
            Loaded += ((s, e) => Initialize());
        }
    }
}
