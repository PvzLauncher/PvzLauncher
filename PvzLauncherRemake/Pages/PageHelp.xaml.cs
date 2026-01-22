using HuaZi.Library.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils.UI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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


                ChangePage(AppGlobals.HelpIndex.Root);
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }
        #endregion

        #region ChangePage
        public async void ChangePage(JsonHelpIndex.CardInfo[] cards)
        {
            try
            {
                stackPanel.Children.Clear();

                foreach (var card in cards)
                {
                    var userBigCard = new UserBigCard
                    {
                        Title = card.Title,
                        Subtitle = card.Subtitle,
                        Icon = GameIconConverter.ParseGameIconToUserControl(GameIconConverter.ParseStringToGameIcons(card.Icon)),
                    };
                    userBigCard.MouseUp += ((s, e) => ChangePage(card.Childrens));
                    stackPanel.Children.Add(userBigCard);
                    userBigCard.FadeIn();

                    await Task.Delay(100);
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
