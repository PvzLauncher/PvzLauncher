using Newtonsoft.Json;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils.UI;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageDeveloper.xaml 的交互逻辑
    /// </summary>
    public partial class PageDeveloper : ModernWpf.Controls.Page
    {
        public PageDeveloper()
        {
            InitializeComponent();
            Loaded += (async (s, e) =>
            {
                try
                {
                    #region 变量指标
                    string varText = "";

                    Type type = typeof(Globals);

                    FieldInfo[] staticFields = type.GetFields(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Static |
                        BindingFlags.DeclaredOnly
                    );

                    foreach (FieldInfo field in staticFields)
                    {
                        string name = field.Name;

                        var value = JsonConvert.SerializeObject(field.GetValue(null), Formatting.Indented);

                        string typeName = field.FieldType.Name;

                        varText = $"{varText}{name}({typeName}): {value}\n\n";
                    }

                    textblock_varinfos.Text = varText;
                    #endregion

                    #region 导航
                    comboBox_navigator.Items.Clear();
                    foreach (var page in Enum.GetNames(typeof(NavigaionPages)))
                        comboBox_navigator.Items.Add(page);
                    if (comboBox_navigator.Items.Count > 0) comboBox_navigator.SelectedIndex = 0;

                    button_navigator.Click += ((s, e) =>
                    {
                        if (Enum.TryParse<NavigaionPages>((string)comboBox_navigator.SelectedItem, out var result))
                            NavigationController.Navigate(result);
                    });
                    #endregion

                    #region 服务器文件下载

                    /*JsonFileIndex.Root index;
                    using (var client = new HttpClient())
                        index = JsonHelper.ReadJson<JsonFileIndex.Root>(await client.GetStringAsync(Globals.Urls.FileIndexUrl));
                    listBox_fileDownload_List.Items.Clear();
                    foreach (var file in index.List)
                        listBox_fileDownload_List.Items.Add($"{file}");

                    listBox_fileDownload_List.SelectionChanged += ((s, e) =>
                    {
                        var selected = index.Files[(string)listBox_fileDownload_List.SelectedItem];

                        textBlock_FileDownload_Info.Text = $"""
                    OriginalFileName: {selected.OriginalFileName}
                    Size: {Math.Round(selected.Size / 1024.0, 2)} KB
                    Url: {selected.Url}
                    """;

                    });

                    button_FileDownload_DOWNLOAD.Click += ((s, e) =>
                    {
                        if (listBox_fileDownload_List.SelectedItem == null)
                        {
                            MessageBox.Show("no selected");
                            return;
                        }

                        var selected = index.Files[(string)listBox_fileDownload_List.SelectedItem];

                        string savePath = Path.Combine(Globals.Directories.TempDirectory, $"PVZLAUNCHER.FILE.DOWNLOAD.CACHE.{Guid.NewGuid():N}");

                        TaskManager.AddTask(new DownloadTaskInfo
                        {
                            Downloader = new DownloadService
                            {
                                Url = selected.Url,
                                SavePath = savePath
                            },
                            TaskName = $"[DEV] 下载 \"{selected.OriginalFileName}\"",
                            TaskIcon = GameIcons.Unknown
                        });

                        SnackbarService.Show(new SnackbarContent
                        {
                            Title = "下载已开始",
                            Content = "",
                            Type = SnackbarType.Info
                        });
                    });*/

                    #endregion
                }
                catch (Exception ex)
                {
                    ErrorReportDialog.Show(ex);
                }
            });

            button_flyoutTEST.Click += (s, e) => flyout1.ShowAt(button_flyoutTEST);

            rating.ValueChanged += (s, e) => SnackbarService.Show(new SnackbarContent
            {
                Title = "result",
                Content = $"{rating.Value}",
                Type = SnackbarType.Success
            });

            #region 教程测试
            int totalStep = 3;
            int currentStep = 0;
            button_TUstart.Click += (s, e) =>
            {
                currentStep = 0;
                currentStep++;
                TutorialManager.ShowTutorial();
                TutorialManager.SetTutorial($"{currentStep}/{totalStep} 这是第一个教程标题", "这两个输入框原本应该作为教程的标题与内容定义，但是被弃用了", textBox_TUtitle);
            };
            TutorialManager.TutorialHost.NextButtonClick += (s, e) =>
            {
                currentStep++;
                string tit = "";
                string txt = "";
                UIElement tgt = null!;
                if (currentStep == 2)
                {
                    tit = "这是第二个教程标题";
                    txt = "因为教程系统目前仍然非常简陋，完全无法正常投入使用，因为只能放在开发者菜单当测试内容";
                    tgt = textBox_TUcontent;
                }
                else if (currentStep == 3)
                {
                    tit = "这是最后一个教程标题";
                    txt = "你可以点击此按钮再次查看此教程";
                    tgt = button_TUstart;
                }
                else if (currentStep > 3)
                {
                    TutorialManager.HideTutorial();
                    return;
                }

                TutorialManager.SetTutorial($"{currentStep}/{totalStep} {tit}", txt, tgt);
            };
            #endregion

            #region UserCard测试
            usercard_UCTmCard.MouseUp += (s, e) =>
            {
                SnackbarService.Show(new SnackbarContent
                {
                    Title = "test",
                    Content = "you clicked card",
                    Type = SnackbarType.Info
                });
            };
            #endregion
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SnackbarService.Show(new SnackbarContent
            {
                Title = "test",
                Content = "CLICKED",
                Type = SnackbarType.Info
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var button = new Button
            {
                Content = "click me !"
            };
            var card = new UserCard
            {
                Title = "new card",
                Description = "",
                Version = "test",
                Size = "test",
                SupportVersion = "test",
                CustomControl = button
            };

            button.Click += (s, e) => SnackbarService.Show(new SnackbarContent
            {
                Title = "test",
                Content = "CLICKED!",
                Type = SnackbarType.Info
            });

            stackPanel_UCTmain.Children.Add(card);
        }
    }
}
