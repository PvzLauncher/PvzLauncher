using HuaZi.Library.Json;
using ModernWpf.Controls;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils.Configuration;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Utils.UI;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using static PvzLauncherRemake.Classes.AppLogger;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageManage.xaml 的交互逻辑
    /// </summary>
    public partial class PageManage : ModernWpf.Controls.Page
    {
        #region Loads
        public void StartLoad()
        {
            grid_Loading.Visibility = Visibility.Visible;
            grid_Main.IsEnabled = false;
            grid_Main.Effect = new BlurEffect { Radius = 10 };
        }

        public void EndLoad()
        {
            grid_Loading.Visibility = Visibility.Hidden;
            grid_Main.IsEnabled = true;
            grid_Main.Effect = null;
        }
        #endregion

        #region Initialize
        public async void Initialize()
        {
            try
            {
                logger.Info($"[管理] 开始初始化...");
                StartLoad();

                //清理
                stackPanel_Game.Children.Clear();
                stackPanel_Trainer.Children.Clear();
                //加载列表
                logger.Info($"[管理] 开始加载游戏列表");
                await GameManager.LoadGameListAsync();
                await GameManager.LoadTrainerListAsync();
                logger.Info($"[管理] 游戏列表加载完毕");

                //游戏库里有东西才加
                if (AppGlobals.GameList.Count > 0)
                {
                    logger.Info($"[管理] 开始添加卡片");
                    //添加卡片
                    foreach (var game in AppGlobals.GameList)
                    {
                        //定义卡片
                        var card = new UserCard
                        {
                            Title = game.GameInfo.Name,
                            Icon = GameIconConverter.ParseStringToGameIcons(game.GameInfo.Icon),
                            isActive = game.GameInfo.Name == AppGlobals.Config.CurrentGame ? true : false,
                            Version = $"{game.GameInfo.Version}",
                            Background = System.Windows.Media.Brushes.Transparent,
                            Tag = game,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        card.MouseDoubleClick += SelectGame;
                        card.MouseRightButtonUp += SetGame;
                        logger.Info($"[管理] 添加游戏: Title=\"{card.Title}\"  Icon=\"{card.Icon}\"  isCurrent=\"{card.isActive}\"  Version=\"{card.Version}\"");
                        stackPanel_Game.Children.Add(card);//添加

                    }
                }
                else
                {
                    logger.Warn($"[管理] 在游戏库内未发现任何游戏");
                    AppGlobals.Config.CurrentGame = null!;

                    grid_NoneGame.Visibility = Visibility.Visible;
                    grid_NoneGame.IsEnabled = true;
                }

                //添加修改器
                //游戏库里有东西才加
                if (AppGlobals.TrainerList.Count > 0)
                {
                    logger.Info($"[管理] 添加修改器列表");
                    //添加卡片
                    foreach (var trainer in AppGlobals.TrainerList)
                    {
                        //定义卡片
                        var card = new UserCard
                        {
                            Title = trainer.Name,
                            Icon = GameIconConverter.ParseStringToGameIcons(trainer.Icon),
                            isActive = trainer.Name == AppGlobals.Config.CurrentTrainer ? true : false,
                            Version = $"{trainer.Version}", //拼接，示例:"英文原版 1.0.0.1051"
                            Background = System.Windows.Media.Brushes.Transparent,
                            Tag = trainer,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        card.MouseDoubleClick += SelectTrainer;
                        card.MouseRightButtonUp += SetTrainer;
                        logger.Info($"[管理] 添加修改器: Title=\"{card.Title}\"  Icon=\"{card.Icon}\"  isCurrent=\"{card.isActive}\"  Version=\"{card.Version}\"");
                        stackPanel_Trainer.Children.Add(card);//添加

                    }
                }
                else
                {
                    logger.Warn($"[管理] 未发现任何修改器");
                    AppGlobals.Config.CurrentTrainer = null!;

                    grid_NoneTrainer.Visibility = Visibility.Visible;
                    grid_NoneTrainer.IsEnabled = true;
                }

                EndLoad();
                logger.Info($"[管理] ");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }
        #endregion

        //tab动画
        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized)
            {
                if (e.OriginalSource != sender)
                    return;

                var selectItem = ((TabControl)sender).SelectedContent;
                Grid animControl = null!;

                if (selectItem is Grid)
                {
                    animControl = (Grid)selectItem;
                }
                else
                {
                    return;
                }

                animControl.BeginAnimation(MarginProperty, null);
                animControl.BeginAnimation(OpacityProperty, null);

                animControl.Margin = new Thickness(0, 25, 0, 0);
                animControl.Opacity = 0;

                var margniAnim = new ThicknessAnimation
                {
                    To = new Thickness(0),
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };
                var opacAnim = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };
                animControl.BeginAnimation(MarginProperty, margniAnim);
                animControl.BeginAnimation(OpacityProperty, opacAnim);
            }
        }

        public PageManage() => InitializeComponent();

        private void Page_Loaded(object sender, RoutedEventArgs e) => Initialize();

        //选择游戏
        private void SelectGame(object sender, MouseButtonEventArgs e)
        {
            try
            {
                SnackbarManager.Show(new SnackbarContent
                {
                    Title = "选择游戏",
                    Content = $"已选择 \"{((UserCard)sender).Title}\" 作为启动游戏",
                    Type = SnackbarType.Info
                });

                //更新控件
                foreach (var card in stackPanel_Game.Children)
                {
                    ((UserCard)card).isActive = (((UserCard)card).Title == ((UserCard)sender).Title);
                    ((UserCard)card).SetLabels();
                }
                foreach (var card in stackPanel_Search.Children)
                {
                    ((UserCard)card).isActive = (((UserCard)card).Title == ((UserCard)sender).Title);
                    ((UserCard)card).SetLabels();
                }

                AppGlobals.Config.CurrentGame = $"{((UserCard)sender).Title}";
                ConfigManager.SaveConfig();
                logger.Info($"[管理] 选择游戏: {AppGlobals.Config.CurrentGame}");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }

        //选择修改器
        private void SelectTrainer(object sender, MouseButtonEventArgs e)
        {
            try
            {
                SnackbarManager.Show(new SnackbarContent
                {
                    Title = "选择修改器",
                    Content = $"已选择 \"{((UserCard)sender).Title}\" 作为当前修改器",
                    Type = SnackbarType.Info
                });

                //更新控件
                foreach (var card in stackPanel_Trainer.Children)
                {
                    ((UserCard)card).isActive = (((UserCard)card).Title == ((UserCard)sender).Title);
                    ((UserCard)card).SetLabels();
                }
                foreach (var card in stackPanel_Search.Children)
                {
                    ((UserCard)card).isActive = (((UserCard)card).Title == ((UserCard)sender).Title);
                    ((UserCard)card).SetLabels();
                }


                AppGlobals.Config.CurrentTrainer = $"{((UserCard)sender).Title}";
                ConfigManager.SaveConfig();
                logger.Info($"[管理] 选择修改器: {AppGlobals.Config.CurrentTrainer}");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }

        //设置游戏
        private void SetGame(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.NavigationService.Navigate(new PageManageSet((JsonGameInfo.Index)((UserCard)sender).Tag));
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }

        //设置修改器
        private async void SetTrainer(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var trainerConfig = (JsonTrainerInfo.Index)(((UserCard)sender).Tag);

                logger.Info($"[管理: 修改器设置] 开始设置修改器");
                //控件
                var buttonDelete = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100)),
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new ModernWpf.Controls.PathIcon
                            {
                                Width=15,
                                Height=15,
                                Data=Geometry.Parse("M280-120q-33 0-56.5-23.5T200-200v-520h-40v-80h200v-40h240v40h200v80h-40v520q0 33-23.5 56.5T680-120H280Zm400-600H280v520h400v-520ZM360-280h80v-360h-80v360Zm160 0h80v-360h-80v360ZM280-720v520-520Z"),
                                Margin=new Thickness(0,0,5,0)
                            },
                            new TextBlock
                            {
                                Text="删除修改器"
                            }
                        }
                    }
                };
                var buttonRename = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 0, 10),
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new ModernWpf.Controls.PathIcon
                            {
                                Width=15,
                                Height=15,
                                Data=Geometry.Parse("M200-200h57l391-391-57-57-391 391v57Zm-80 80v-170l528-527q12-11 26.5-17t30.5-6q16 0 31 6t26 18l55 56q12 11 17.5 26t5.5 30q0 16-5.5 30.5T817-647L290-120H120Zm640-584-56-56 56 56Zm-141 85-28-29 57 57-29-28Z"),
                                Margin=new Thickness(0,0,5,0)
                            },
                            new TextBlock
                            {
                                Text="修改名称"
                            }
                        }
                    }
                };
                var buttonOpenFolder = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 0, 10),
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new ModernWpf.Controls.PathIcon
                            {
                                Width=15,
                                Height=15,
                                Data=Geometry.Parse("M160-160q-33 0-56.5-23.5T80-240v-480q0-33 23.5-56.5T160-800h240l80 80h320q33 0 56.5 23.5T880-640H447l-80-80H160v480l96-320h684L837-217q-8 26-29.5 41.5T760-160H160Zm84-80h516l72-240H316l-72 240Zm0 0 72-240-72 240Zm-84-400v-80 80Z"),
                                Margin=new Thickness(0,0,5,0)
                            },
                            new TextBlock
                            {
                                Text="打开文件夹"
                            }
                        }
                    }
                };

                var dialog = new ContentDialog
                {
                    Title = "操作",
                    Content = new StackPanel
                    {
                        Children =
                        {
                            buttonDelete,buttonRename,buttonOpenFolder
                        }
                    },
                    CloseButtonText = "关闭",
                    DefaultButton = ContentDialogButton.Close
                };

                //删除
                buttonDelete!.Click += (async (s, e) =>
                {
                    logger.Info($"[管理: 修改器设置] 开始删除修改器");
                    dialog.Hide();

                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "确认删除",
                        Content = $"\"{trainerConfig.Name}\" 将被删除，一旦删除将永久消失(真的很久!)\n\n(此操作仅有这一次确认机会，点击删除按钮立即执行删除程序！)",
                        PrimaryButtonText = "删除",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Close
                    }, (async () =>
                    {
                        logger.Info($"[管理: 修改器设置] 用户同意删除，开始删除...");

                        await Task.Run(() => Directory.Delete(Path.Combine(AppGlobals.TrainerDirectory, trainerConfig.Name), true));
                        logger.Info($"[管理: 修改器设置] 删除完毕");
                        await GameManager.LoadTrainerListAsync();

                        if (AppGlobals.TrainerList.Count > 0 && AppGlobals.Config.CurrentTrainer == trainerConfig.Name)
                        {
                            AppGlobals.Config.CurrentTrainer = AppGlobals.TrainerList[0].Name;
                        }
                        else
                        {
                            AppGlobals.Config.CurrentTrainer = null!;
                        }
                        ConfigManager.SaveConfig();
                        SnackbarManager.Show(new SnackbarContent
                        {
                            Title = "删除成功",
                            Content = $"\"{trainerConfig.Name}\" 已从您的修改器库内移除!",
                            Type = SnackbarType.Success
                        });

                        this.NavigationService.Refresh();
                    }));
                });

                //重命名
                buttonRename!.Click += (async (s, e) =>
                {
                    logger.Info($"[管理: 修改器设置] 开始重命名...");
                    dialog.Hide();
                    var textBox = new TextBox
                    {
                        Text = trainerConfig.Name
                    };

                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "重命名",
                        Content = textBox,
                        PrimaryButtonText = "确定",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Primary
                    }, (() =>
                    {
                        if (textBox.Text != null)
                        {
                            if (!Directory.Exists(Path.Combine(AppGlobals.TrainerDirectory, textBox.Text)))
                            {
                                string lastName = trainerConfig.Name;
                                trainerConfig.Name = textBox.Text;
                                Directory.Move(Path.Combine(AppGlobals.TrainerDirectory, lastName), Path.Combine(AppGlobals.TrainerDirectory, trainerConfig.Name));
                                Json.WriteJson(Path.Combine(AppGlobals.TrainerDirectory, trainerConfig.Name, ".pvzl.json"), trainerConfig);
                                SnackbarManager.Show(new SnackbarContent
                                {
                                    Title = "更名成功",
                                    Content = $"修改器已更名为: {trainerConfig.Name}",
                                    Type = SnackbarType.Success
                                });

                                logger.Info($"[管理: 修改器设置] 更名成功: {trainerConfig.Name}");

                                if (AppGlobals.Config.CurrentTrainer == lastName)
                                    AppGlobals.Config.CurrentTrainer = trainerConfig.Name;

                                this.NavigationService.Refresh();
                            }
                            else
                            {
                                logger.Info($"[管理: 修改器设置] {textBox.Text} 在库内已有相同名称，操作取消");
                                SnackbarManager.Show(new SnackbarContent
                                {
                                    Title = "更名失败",
                                    Content = $"库内已有与 \"{textBox.Text}\" 同名修改器！",
                                    Type = SnackbarType.Error
                                });
                            }
                        }
                        else
                        {
                            logger.Info($"[管理: 修改器设置] 用户输入为空，操作取消");
                            SnackbarManager.Show(new SnackbarContent
                            {
                                Title = "更名失败",
                                Content = "新名称为空",
                                Type = SnackbarType.Error
                            });
                        }
                    }));
                });

                //打开文件夹
                buttonOpenFolder!.Click += ((s, e) =>
                {
                    dialog.Hide();
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Path.Combine(AppGlobals.TrainerDirectory, trainerConfig.Name),
                        UseShellExecute = true
                    });
                });

                await DialogManager.ShowDialogAsync(dialog);

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }

        //导入游戏
        //我已经懒得碰这坨该死的屎山了，能跑就完事了
        // :(
        //幸运的是，我现在已经开始重构他了！！(2026-1-3)
        private async void button_ImportGame_Click(object sender, RoutedEventArgs e)
        {
            StartLoad();

            await GameManager.ImportGameOrTrainer(((progress) => textBlock_Loading.Text = $"正在复制: {progress}"));

            NavigationService.Refresh();

            EndLoad();
        }




        //搜索
        private void button_Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (stackPanel_Game.Children.Count <= 0 && stackPanel_Trainer.Children.Count <= 0)
                    return;

                stackPanel_Search.Children.Clear();

                var allItem = new List<UserCard>();
                allItem.Clear();

                foreach (var item in stackPanel_Game.Children)
                    if (item is UserCard card)
                        allItem.Add(card);
                foreach (var item in stackPanel_Trainer.Children)
                    if (item is UserCard card)
                        allItem.Add(card);



                //寻找
                foreach (var item in allItem)
                {
                    if (item.Title.Contains(textBox_Search.Text))
                    {
                        var card = new UserCard
                        {
                            Title = item.Title,
                            Icon = item.Icon,
                            isActive = item.isActive,
                            Version = item.Version,
                            Background = item.Background,
                            Tag = item.Tag,
                            Margin = item.Margin
                        };
                        if (card.Tag is JsonGameInfo.Index)
                        {
                            card.MouseDoubleClick += SelectGame;
                            card.MouseRightButtonUp += SetGame;
                        }
                        else if (card.Tag is JsonTrainerInfo.Index)
                        {
                            card.MouseDoubleClick += SelectTrainer;
                            card.MouseRightButtonUp += SetTrainer;
                        }

                        stackPanel_Search.Children.Add(card);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }

        private void textBox_Search_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                button_Search_Click(button_Search, null!);
        }

        private void Button_Click(object sender, RoutedEventArgs e) => NavigationController.Navigate(NavigaionPages.Download);
    }
}
