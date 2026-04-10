using HuaZi.Library.Downloader;
using HuaZi.Library.Json;
using Newtonsoft.Json;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Classes.JsonConfigs;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Utils.UI;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Policy;
using System.Windows;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageDeveloper.xaml 的交互逻辑
    /// </summary>
    public partial class PageDeveloper : ModernWpf.Controls.Page
    {
        public async void Initialize()
        {
            try
            {
                #region 变量指标
                string varText = "";

                Type type = typeof(AppGlobals);

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

                JsonFileIndex.Index index;
                using (var client = new HttpClient())
                    index = Json.ReadJson<JsonFileIndex.Index>(await client.GetStringAsync(AppGlobals.Urls.FileIndexUrl));
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

                    string savePath = Path.Combine(AppGlobals.Directories.TempDiectory, $"PVZLAUNCHER.FILE.DOWNLOAD.CACHE.{new Random().Next(int.MinValue, int.MaxValue)}");

                    TaskManager.AddTask(new DownloadTaskInfo
                    {
                        Downloader = new Downloader
                        {
                            Url = selected.Url,
                            SavePath = savePath
                        },
                        TaskName = $"[DEV] 下载 \"{selected.OriginalFileName}\"",
                        TaskType = TaskType.File,
                        TaskIcon = GameIcons.Unknown
                    });

                    SnackbarManager.Show(new SnackbarContent
                    {
                        Title = "下载已开始",
                        Content = "",
                        Type = SnackbarType.Info
                    });
                });

                #endregion
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show(ex);
            }
        }


        public PageDeveloper()
        {
            InitializeComponent();
            Loaded += ((s, e) => Initialize());
        }
    }
}
