using Microsoft.Win32;
using ModernWpf;
using ModernWpf.Controls;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using PvzLauncherRemake.Utils;
using PvzLauncherRemake.Utils.FileSystem;
using PvzLauncherRemake.Utils.Network;
using PvzLauncherRemake.Utils.UI;
using PvzLauncherRemake.Windows;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageSettings.xaml 的交互逻辑
    /// </summary>
    public partial class PageSettings : ModernWpf.Controls.Page
    {
        private bool isInitialized = false;

        public void ShowRestartTip()
        {
            SnackbarService.Show(new SnackbarContent
            {
                Title = "提示",
                Content = "此设置项重启才能生效",
                Type = SnackbarType.Info
            });
        }

        #region Animation
        public async Task StartAnimation()
        {
            List<Panel> animationStackPanels = new List<Panel>();

            foreach (var controls in VisualTreeTools.GetVisualChildren(this))
            {
                if (controls is Panel sp && sp.Tag != null && sp.Tag.ToString() == "aniSp")
                {
                    animationStackPanels.Add(sp);

                    sp.RenderTransform = new TranslateTransform { X = -sp.ActualWidth, Y = 0 };
                    sp.Opacity = 0;
                }
            }

            foreach (var sp in animationStackPanels)
            {
                await Task.Delay(50);
                StackPanelFadeIn(sp);
            }
        }

        public void StackPanelFadeIn(Panel sp)
        {
            ((TranslateTransform)sp.RenderTransform).BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation
            {
                From = -sp.ActualWidth,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            });
            sp.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            });
        }
        #endregion

        public PageSettings()
        {
            InitializeComponent();
            Loaded += (async (s, e) =>
            {
                try
                {
                    isInitialized = false;

                    //# 启动器设置
                    //## 操作
                    //### 游戏启动后操作
                    switch (Globals.Config.Settings.LauncherConfig.LaunchedOperate)
                    {
                        case "None":
                            comboBox_LaunchedOperate.SelectedIndex = 0; break;
                        case "Close":
                            comboBox_LaunchedOperate.SelectedIndex = 1; break;
                        case "HideAndDisplay":
                            comboBox_LaunchedOperate.SelectedIndex = 2; break;
                    }
                    //### 修改器随游戏启动
                    checkbox_Launcher_LaunchWithTrainer.IsChecked = Globals.Config.Settings.LauncherConfig.LaunchWithTrainer;
                    //### 管理选择模式
                    radioButton_Launcher_ManageSelectMode_Single.IsChecked = false;
                    radioButton_Launcher_ManageSelectMode_Double.IsChecked = false;
                    switch (Globals.Config.Settings.LauncherConfig.ManageSelectMode)
                    {
                        case "Single":
                            radioButton_Launcher_ManageSelectMode_Single.IsChecked = true; break;
                        case "Double":
                            radioButton_Launcher_ManageSelectMode_Double.IsChecked = true; break;
                    }
                    //## 外观
                    //### 主题
                    radioButton_Theme_Light.IsChecked = false;
                    radioButton_Theme_Dark.IsChecked = false;
                    switch (Globals.Config.Settings.LauncherConfig.Theme)
                    {
                        case "Light":
                            radioButton_Theme_Light.IsChecked = true; break;
                        case "Dark":
                            radioButton_Theme_Dark.IsChecked = true; break;
                    }
                    //### 语言
                    foreach (ComboBoxItem item in comboBox_Launcher_Language.Items)
                    {
                        if (item.Tag.ToString() == Globals.Config.Settings.LauncherConfig.Language)
                            comboBox_Launcher_Language.SelectedItem = item;
                    }
                    //### 窗口标题
                    textBox_WindowTitle.Text = Globals.Config.Settings.LauncherConfig.WindowTitle;
                    //### 标题图片
                    radioButton_TitieImage_EN.IsChecked = false; radioButton_TitleImage_ZH.IsChecked = false;
                    switch (Globals.Config.Settings.LauncherConfig.TitleImage)
                    {
                        case "EN":
                            radioButton_TitieImage_EN.IsChecked = true; break;
                        case "ZH":
                            radioButton_TitleImage_ZH.IsChecked = true; break;
                    }
                    //### 背景
                    radioButton_Background_Default.IsChecked = false; radioButton_Background_Custom.IsChecked = false;
                    switch (Globals.Config.Settings.LauncherConfig.BackgroundMode)
                    {
                        case "default": radioButton_Background_Default.IsChecked = true; break;
                        case "custom": radioButton_Background_Custom.IsChecked = true; break;
                    }

                    if (Globals.Caches.LauncherBackground != null)
                    {
                        image_Background.Source = Globals.Caches.LauncherBackground;
                    }
                    //### 回声洞
                    //checkBox_Launcher_EchoCave.IsChecked = Globals.Config.Settings.LauncherConfig.EchoCaveEnabled;
                    //### 公告
                    checkBox_Launcher_Notice.IsChecked = Globals.Config.Settings.LauncherConfig.NoticeEnabled;
                    //### 启动动画
                    checkBox_Launcher_LaunchAnimaiton.IsChecked = Globals.Config.Settings.LauncherConfig.LaunchAnimationEnabled;
                    //### NavigationView位置
                    radioButton_NavViewLeft.IsChecked = false; radioButton_NavViewTop.IsChecked = false;
                    switch (Globals.Config.Settings.LauncherConfig.NavigationViewAlign)
                    {
                        case "Left":
                            radioButton_NavViewLeft.IsChecked = true; break;
                        case "Top":
                            radioButton_NavViewTop.IsChecked = true; break;
                    }
                    //## 网络
                    //### 提供方
                    switch (Globals.Config.Settings.LauncherConfig.ServiceProvider)
                    {
                        case "Gitee":
                            comboBox_Launcher_ServiceProvider.SelectedIndex = 0; break;
                        case "GitCode":
                            comboBox_Launcher_ServiceProvider.SelectedIndex = 1; break;
                        case "Github":
                            comboBox_Launcher_ServiceProvider.SelectedIndex = 2; break;
                    }
                    //### 下载引擎线程数
                    slider_Network_ThreadCount.Value = Globals.Config.Settings.LauncherConfig.DownloadEngineThreadCount;
                    textBlock_Network_ThreadCount.Text = Globals.Config.Settings.LauncherConfig.DownloadEngineThreadCount.ToString();
                    //### 离线模式
                    checkBox_Network_OfflineMode.IsChecked = Globals.Config.Settings.LauncherConfig.OfflineMode;
                    //## 更新
                    //### 更新通道
                    if (!Globals.IsStable)
                    {
                        comboBox_UpdateChannel.IsEnabled = false;
                        Globals.Config.Settings.LauncherConfig.UpdateChannel = "Development";
                    }
                    switch (Globals.Config.Settings.LauncherConfig.UpdateChannel)
                    {
                        case "Stable":
                            comboBox_UpdateChannel.SelectedIndex = 0; break;
                        case "Development":
                            comboBox_UpdateChannel.SelectedIndex = 1; break;
                    }
                    //### 启动时检查更新
                    checkBox_StartUpCheckUpdate.IsChecked = Globals.Config.Settings.LauncherConfig.StartUpCheckUpdate;


                    //# 游戏设置
                    //## 游戏配置
                    //### 全屏
                    switch (Globals.Config.Settings.GameConfig.FullScreen)
                    {
                        case "Default":
                            comboBox_Game_FullScreen.SelectedIndex = 0; break;
                        case "FullScreen":
                            comboBox_Game_FullScreen.SelectedIndex = 1; break;
                        case "Windowed":
                            comboBox_Game_FullScreen.SelectedIndex = 2; break;
                    }
                    //### 位置
                    switch (Globals.Config.Settings.GameConfig.StartUpLocation)
                    {
                        case "Default":
                            comboBox_Game_Location.SelectedIndex = 0; break;
                        case "Center":
                            comboBox_Game_Location.SelectedIndex = 1; break;
                        case "LeftTop":
                            comboBox_Game_Location.SelectedIndex = 2; break;
                    }
                    //### 3D加速
                    switch (Globals.Config.Settings.GameConfig.ThreeDMode)
                    {
                        case "Default":
                            comboBox_Game_3DMode.SelectedIndex = 0; break;
                        case "On":
                            comboBox_Game_3DMode.SelectedIndex = 1; break;
                        case "Off":
                            comboBox_Game_3DMode.SelectedIndex = 2; break;
                    }
                    //## 外观
                    //### 窗口标题
                    textBox_GameWindowTitle.Text = Globals.Config.Settings.GameConfig.WindowTitle;
                    //## 覆盖界面
                    //### 启用
                    checkbox_Game_Overlay_Enabled.IsChecked = Globals.Config.Settings.GameConfig.OverlayUIEnabled;

                    //# 存档设置
                    //## 存档隔离
                    //### 启用存档隔离
                    checkBox_EnableIsolationSave.IsChecked = Globals.Config.Settings.SaveConfig.EnableSaveIsolation;










                    await StartAnimation();

                    isInitialized = true;

                }
                catch (Exception ex)
                {
                    ErrorReportDialog.Show(ex);
                }
            });

            tabControl.SelectionChanged += TabControlAnimationHelper.TabControlAnimtion;
        }



        #region 启动器设置

        private void Launcher_LaunchOperate(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                switch (comboBox_LaunchedOperate.SelectedIndex)
                {
                    case 0:
                        Globals.Config.Settings.LauncherConfig.LaunchedOperate = "None"; break;
                    case 1:
                        Globals.Config.Settings.LauncherConfig.LaunchedOperate = "Close"; break;
                    case 2:
                        Globals.Config.Settings.LauncherConfig.LaunchedOperate = "HideAndDisplay"; break;
                }
                ConfigManager.SaveConfig();
            }
        }

        private void Launcher_LaunchWithTrainer(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.LauncherConfig.LaunchWithTrainer = checkbox_Launcher_LaunchWithTrainer.IsChecked == true ? true : false;
                ConfigManager.SaveConfig();
            }
        }

        private void Launcher_ManageSelectMode(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.LauncherConfig.ManageSelectMode = (string)(((RadioButton)sender).Tag);
                ConfigManager.SaveConfig();
            }
        }

        private void Launcher_Theme(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.LauncherConfig.Theme = (string)(((RadioButton)sender).Tag);
                ConfigManager.SaveConfig();
                switch (Globals.Config.Settings.LauncherConfig.Theme)
                {
                    case "Light":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light; break;
                    case "Dark":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark; break;
                }
            }
        }

        private void Launcher_Language(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                var comboBox = (ComboBox)sender;
                Globals.Config.Settings.LauncherConfig.Language = ((ComboBoxItem)comboBox.SelectedItem).Tag.ToString()!;
                ConfigManager.SaveConfig();

                LocalizeService.SwitchLanguage(Globals.Config.Settings.LauncherConfig.Language);
            }
        }

        private void Launcher_TitleReset(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
                textBox_WindowTitle.Text = new JsonConfig.Root().Settings.LauncherConfig.WindowTitle;
        }

        private void Launcher_Title(object sender, TextChangedEventArgs e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.LauncherConfig.WindowTitle = textBox_WindowTitle.Text;
                ((WindowMain)Window.GetWindow(this)).Title = Globals.Config.Settings.LauncherConfig.WindowTitle;
                ConfigManager.SaveConfig();
            }
        }

        private void Launcher_TitleImageLanguage(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (sender is RadioButton radioButton)
                {
                    if ((string)radioButton.Tag == "EN")
                        radioButton_TitleImage_ZH.IsChecked = false;
                    if ((string)radioButton.Tag == "ZH")
                        radioButton_TitieImage_EN.IsChecked = false;

                    Globals.Config.Settings.LauncherConfig.TitleImage = (string)radioButton.Tag;
                    ConfigManager.SaveConfig();
                }


            }
        }

        private void Launcher_BackgroundCustom(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "图像文件|*.png;*.jpg;*.webp;*.bmp",
                    Multiselect = false,
                    CheckFileExists = true
                };
                if (dialog.ShowDialog() == true)
                {
                    File.Copy(dialog.FileName, Globals.Paths.BackgroundPath, true);
                    Globals.Caches.LauncherBackground = BitmapHelper.LoadBitmapImageFromDisk(Globals.Paths.BackgroundPath);
                    image_Background.Source = Globals.Caches.LauncherBackground;
                }
            }
        }

        private async void Launcher_BackgroundSelect(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (sender is RadioButton radioButton)
                {
                    if ((string)radioButton.Tag == "Default")
                    {
                        Globals.Config.Settings.LauncherConfig.BackgroundMode = "default";
                        radioButton_Background_Custom.IsChecked = false;
                    }
                    if ((string)radioButton.Tag == "Custom")
                    {
                        if (!File.Exists(Globals.Paths.BackgroundPath))
                        {
                            await DialogService.ShowDialogAsync(new ContentDialog
                            {
                                Title = "警告",
                                Content = "自定义背景图不存在，请先点击下方选择一个图片",
                                PrimaryButtonText = "确定",
                                DefaultButton = ContentDialogButton.Primary
                            });
                            radioButton_Background_Custom.IsChecked = false;
                        }
                        else
                        {
                            Globals.Config.Settings.LauncherConfig.BackgroundMode = "custom";
                            radioButton_Background_Default.IsChecked = false;
                        }
                    }
                    ConfigManager.SaveConfig();
                }


            }
        }

        /*private void Launcher_EchoCave(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (sender is not CheckBox checkBox)
                    return;

                Globals.Config.Settings.LauncherConfig.EchoCaveEnabled = checkBox.IsChecked == true ? true : false;
                ConfigManager.SaveConfig();
            }
        }*/

        private void Launcher_Notice(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (sender is not CheckBox checkBox)
                    return;

                Globals.Config.Settings.LauncherConfig.NoticeEnabled = checkBox.IsChecked == true ? true : false;
                ConfigManager.SaveConfig();
            }
        }

        private void Launcher_LaunchAnimation(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (sender is not CheckBox checkBox)
                    return;

                Globals.Config.Settings.LauncherConfig.LaunchAnimationEnabled = checkBox.IsChecked == true ? true : false;
                ConfigManager.SaveConfig();
            }
        }

        private void Launcher_NavigationViewAlign(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.LauncherConfig.NavigationViewAlign = (string)(((RadioButton)sender).Tag);
                ConfigManager.SaveConfig();

                switch (Globals.Config.Settings.LauncherConfig.NavigationViewAlign)
                {
                    case "Left": ((NavigationView)Window.GetWindow(this).FindName("navView")).PaneDisplayMode = NavigationViewPaneDisplayMode.Left; break;
                    case "Top": ((NavigationView)Window.GetWindow(this).FindName("navView")).PaneDisplayMode = NavigationViewPaneDisplayMode.Top; break;
                }
            }
        }

        private void Launcher_ServiceProvider(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.LauncherConfig.ServiceProvider = (string)(((ComboBoxItem)(comboBox_Launcher_ServiceProvider.SelectedItem)).Tag);
                ConfigManager.SaveConfig();
                ShowRestartTip();
            }
        }

        private void slider_Network_ThreadCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.LauncherConfig.DownloadEngineThreadCount = (int)slider_Network_ThreadCount.Value;
                ConfigManager.SaveConfig();
                textBlock_Network_ThreadCount.Text = slider_Network_ThreadCount.Value.ToString();
            }
        }

        private void Launcher_OfflineMode(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (sender is CheckBox cb)
                {
                    Globals.Config.Settings.LauncherConfig.OfflineMode = cb.IsChecked == true ? true : false;
                    ConfigManager.SaveConfig();

                    ShowRestartTip();
                }
            }
        }

        private async void Launcher_CheckUpdate(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button senderBtn)
                    return;

                senderBtn.IsEnabled = false;

                await Updater.CheckUpdate((p, s) =>
                {
                    Globals.WindowMain.SetLoadText($"下载更新文件中 {Math.Round(p, 2)}% ... ({Math.Round(s / 1024, 2)} MB/S)");
                });

                senderBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }

        }

        private void Launcher_UpdateChannel(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.LauncherConfig.UpdateChannel = (string)(((ComboBoxItem)comboBox_UpdateChannel.SelectedItem).Tag);
                ConfigManager.SaveConfig();
            }
        }

        private void Launcher_StartUpCheckUpdate(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.LauncherConfig.StartUpCheckUpdate = (bool)checkBox_StartUpCheckUpdate.IsChecked!;
                ConfigManager.SaveConfig();
            }
        }

        private async void Launcher_ClearTemp(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (sender is not Button senderBtn)
                    return;

                senderBtn.IsEnabled = false;
                try
                {
                    Globals.WindowMain.SetLoadState(true, "扫描垃圾文件中...");

                    var trashFiles = await Cleaner.Scan();

                    if (trashFiles.TotalSize <= 0)
                    {
                        SnackbarService.Show(new SnackbarContent
                        {
                            Title = "垃圾清理",
                            Content = "没有找到垃圾文件",
                            Type = SnackbarType.Success
                        });
                        Globals.WindowMain.SetLoadState(false);
                        senderBtn.IsEnabled = true;
                        return;
                    }

                    bool isContinue = false;
                    await DialogService.ShowDialogAsync(new ContentDialog
                    {
                        Title = "垃圾清理",
                        Content = $"已找到 {trashFiles.TempFiles.Length} 个临时文件，{trashFiles.OldLogs.Length} 个过时日志。总大小 {Math.Round(trashFiles.TotalSize / 1024.0 / 1024.0, 2)} MB",
                        PrimaryButtonText = "清理",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Primary
                    }, () => isContinue = true);
                    if (!isContinue)
                    {
                        Globals.WindowMain.SetLoadState(false);
                        senderBtn.IsEnabled = true;
                        return;
                    }

                    Globals.WindowMain.SetLoadText("清理垃圾文件中...");
                    Cleaner.Clean(trashFiles, (f) => Globals.WindowMain.SetLoadText($"清理: {Path.GetFileName(f)}"));

                    SnackbarService.Show(new SnackbarContent
                    {
                        Title = "垃圾清理",
                        Content = $"清理完毕，共 {Math.Round(trashFiles.TotalSize / 1024.0 / 1024.0, 2)} MB",
                        Type = SnackbarType.Success
                    });
                }
                catch (Exception ex)
                {
                    ErrorReportDialog.Show(ex);
                }
                finally
                {
                    Globals.WindowMain.SetLoadState(false);
                    senderBtn.IsEnabled = true;
                }
            }
        }

        #endregion

        #region 游戏设置

        private void Game_FullScreen(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.GameConfig.FullScreen = ((ComboBoxItem)((ComboBox)sender).SelectedItem).Tag.ToString()!;
                ConfigManager.SaveConfig();
            }
        }

        private void Game_Location(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.GameConfig.StartUpLocation = ((ComboBoxItem)((ComboBox)sender).SelectedItem).Tag.ToString()!;
                ConfigManager.SaveConfig();
            }
        }

        private void Game_3DMode(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.GameConfig.ThreeDMode = ((ComboBoxItem)((ComboBox)sender).SelectedItem).Tag.ToString()!;
                ConfigManager.SaveConfig();
            }
        }

        private void Game_WindowTitle(object sender, TextChangedEventArgs e)
        {
            if (isInitialized)
            {
                Globals.Config.Settings.GameConfig.WindowTitle = textBox_GameWindowTitle.Text;
                ConfigManager.SaveConfig();
            }
        }

        private async void Game_OverlayUIEnabled(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (checkbox_Game_Overlay_Enabled.IsChecked == true && !HotKeyHelper.IsHotKeyAvaiable(Key.P, ModifierKeys.Control | ModifierKeys.Alt)) 
                {
                    checkbox_Game_Overlay_Enabled.IsChecked = false;
                    Globals.Config.Settings.GameConfig.OverlayUIEnabled = false;
                    await DialogService.ShowDialogAsync(new ContentDialog
                    {
                        Title = "警告",
                        Content = "检测到用于开启覆盖界面的热键: Ctrl+Alt+P 被占用！\n\n请检查该热键被某个程序占用并取消绑定。确保该热键空闲后再次尝试开启此功能",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    });
                }
                else
                {
                    Globals.Config.Settings.GameConfig.OverlayUIEnabled = checkbox_Game_Overlay_Enabled.IsChecked ?? false;
                }
                ConfigManager.SaveConfig();
            }
        }

        #endregion

        #region 存档设置

        private async void Save_DeleteSave(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                await DialogService.ShowDialogAsync(new ContentDialog
                {
                    Title = "警告",
                    Content = "此操作不可逆，一旦删除您的存档将会永久删除！(真的很久!)",
                    PrimaryButtonText = "删除",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Close
                }, (async () =>
                {
                    await DialogService.ShowDialogAsync(new ContentDialog
                    {
                        Title = "最后一次警告",
                        Content = "这将是最后一次警告，确认后存档立即删除，您现在还有取消的机会！",
                        PrimaryButtonText = "继续删除",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Close
                    }, (async () =>
                    {
                        try
                        {

                            if (Directory.Exists(Globals.Directories.SaveDirectory))
                            {
                                Globals.WindowMain.SetLoadState(true, "删除存档中...");
                                await Task.Run(() =>
                                {
                                    Directory.Delete(Globals.Directories.SaveDirectory, true);
                                });
                                Globals.WindowMain.SetLoadState(false);
                                SnackbarService.Show(new SnackbarContent
                                {
                                    Title = "删除存档",
                                    Content = "您的存档已经移除",
                                    Type = SnackbarType.Success
                                });
                            }
                            else
                            {
                                SnackbarService.Show(new SnackbarContent
                                {
                                    Title = "失败",
                                    Content = "存档不存在，无法删除",
                                    Type = SnackbarType.Error
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorReportDialog.Show(ex);
                        }

                    }));
                }));
            }
        }

        private async void Save_EnabledSaveIsolation(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (checkBox_EnableIsolationSave.IsChecked == true)
                    await DialogService.ShowDialogAsync(new ContentDialog
                    {
                        Title = "警告",
                        Content = "开启存档隔离会导致当前存档丢失。请做好备份再开启！",
                        PrimaryButtonText = "继续开启",
                        SecondaryButtonText = "取消",
                        DefaultButton = ContentDialogButton.Primary
                    }, (() =>
                    {
                        Globals.Config.Settings.SaveConfig.EnableSaveIsolation = true;
                    }), (() =>
                    {
                        checkBox_EnableIsolationSave.IsChecked = false;
                        Globals.Config.Settings.SaveConfig.EnableSaveIsolation = false;
                    }), (() =>
                    {
                        checkBox_EnableIsolationSave.IsChecked = false;
                        Globals.Config.Settings.SaveConfig.EnableSaveIsolation = false;
                    }));
                else
                    Globals.Config.Settings.SaveConfig.EnableSaveIsolation = false;

                ConfigManager.SaveConfig();
            }
        }

        private async void Save_Move(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (Globals.Config.Settings.SaveConfig.EnableSaveIsolation)
                {
                    if (Globals.Caches.GameList.Count >= 2)
                    {
                        var listBox = new ListBox { Margin = new Thickness(0, 20, 0, 0) };
                        string originGameName = null!;
                        string targetGameName = null!;
                        foreach (var game in Globals.Caches.GameList)
                        {
                            listBox.Items.Add(game.GameInfo.Name);
                        }

                        await DialogService.ShowDialogAsync(new ContentDialog
                        {
                            Title = "存档迁移",
                            Content = new Grid
                            {
                                Children =
                            {
                                new TextBlock
                                {
                                    Text="存档迁移可以将某个游戏的存档复制到另一个游戏，请选择要复制的游戏",
                                    VerticalAlignment=VerticalAlignment.Top,
                                    HorizontalAlignment=HorizontalAlignment.Stretch
                                },
                                listBox
                            }
                            },
                            PrimaryButtonText = "确定",
                            CloseButtonText = "取消",
                            DefaultButton = ContentDialogButton.Primary
                        }, (async () =>
                        {
                            if (listBox.SelectedItem != null)
                            {
                                originGameName = listBox.SelectedItem.ToString()!;

                                if (Directory.Exists(Path.Combine(Globals.Directories.GameDirectory, originGameName, ".save")))
                                {

                                    var targetListBox = new ListBox { Margin = new Thickness(0, 20, 0, 0) };
                                    foreach (var game in Globals.Caches.GameList)
                                    {
                                        if (game.GameInfo.Name != originGameName)
                                            targetListBox.Items.Add(game.GameInfo.Name);
                                    }

                                    await DialogService.ShowDialogAsync(new ContentDialog
                                    {
                                        Title = "存档迁移",
                                        Content = new Grid
                                        {
                                            Children =
                                        {
                                            new TextBlock
                                            {
                                                Text="请选择要替换的游戏存档，此操作会将目标游戏的存档覆盖！",
                                                VerticalAlignment=VerticalAlignment.Top,
                                                HorizontalAlignment=HorizontalAlignment.Stretch
                                            },
                                            targetListBox
                                        }
                                        },
                                        PrimaryButtonText = "确定",
                                        CloseButtonText = "取消",
                                        DefaultButton = ContentDialogButton.Primary
                                    }, (async () =>
                                    {
                                        if (targetListBox.SelectedItem != null)
                                        {
                                            targetGameName = targetListBox.SelectedItem.ToString()!;

                                            await DialogService.ShowDialogAsync(new ContentDialog
                                            {
                                                Title = "操作确认",
                                                Content = new StackPanel
                                                {
                                                    Children =
                                                    {
                                                    new TextBlock
                                                    {
                                                        Text="请确认操作，此操作会将原游戏的存档复制到目标游戏\n这会导致目标游戏的存档被覆盖！",
                                                        Margin=new Thickness(0,0,0,5)
                                                    },
                                                    new TextBlock
                                                    {
                                                        Text=$"{originGameName} -> {targetGameName}",
                                                        HorizontalAlignment=HorizontalAlignment.Center
                                                    }
                                                    }
                                                },
                                                PrimaryButtonText = "确认",
                                                CloseButtonText = "取消",
                                                DefaultButton = ContentDialogButton.Primary
                                            }, (async () =>
                                            {
                                                Globals.WindowMain.SetLoadState(true, "迁移存档中...");

                                                await Task.Run(() =>
                                                {
                                                    if (Directory.Exists(Path.Combine(Globals.Directories.GameDirectory, targetGameName, ".save")))
                                                        Directory.Delete(Path.Combine(Globals.Directories.GameDirectory, targetGameName, ".save"), true);
                                                    else
                                                        Directory.CreateDirectory(Path.Combine(Globals.Directories.GameDirectory, targetGameName, ".save"));
                                                });
                                                await DirectoryHelper.CopyDirectoryAsync(Path.Combine(Globals.Directories.GameDirectory, originGameName, ".save"), Path.Combine(Globals.Directories.GameDirectory, targetGameName, ".save"));

                                                SnackbarService.Show(new SnackbarContent
                                                {
                                                    Title = "迁移成功",
                                                    Content = $"{originGameName} 的存档已迁移至 {targetGameName}",
                                                    Type = SnackbarType.Success
                                                });

                                                Globals.WindowMain.SetLoadState(false);
                                            }));
                                        }
                                        else
                                        {
                                            SnackbarService.Show(new SnackbarContent
                                            {
                                                Title = "操作中断",
                                                Content = "没有选择目标游戏",
                                                Type = SnackbarType.Error
                                            });
                                        }
                                    }));
                                }
                                else
                                {
                                    SnackbarService.Show(new SnackbarContent
                                    {
                                        Title = "操作中断",
                                        Content = "原游戏无独立存档，请至少启动一次游戏并创建存档",
                                        Type = SnackbarType.Error
                                    });
                                }
                            }
                            else
                            {
                                SnackbarService.Show(new SnackbarContent
                                {
                                    Title = "操作中断",
                                    Content = "没有选择任何游戏",
                                    Type = SnackbarType.Error
                                });
                            }
                        }));
                    }
                    else
                    {
                        SnackbarService.Show(new SnackbarContent
                        {
                            Title = "无法迁移",
                            Content = "游戏库内少于两个游戏，无法使用此功能",
                            Type = SnackbarType.Warn
                        });
                    }
                }
                else
                {
                    SnackbarService.Show(new SnackbarContent
                    {
                        Title = "提示",
                        Content = "请先启用存档隔离功能",
                        Type = SnackbarType.Warn
                    });
                }
            }
        }



        #endregion
    }
}
