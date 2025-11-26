using Notifications.Wpf;
using PvzLauncherRemake.Class;
using System.Reflection;
using System.Windows.Controls;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageDeveloper.xaml 的交互逻辑
    /// </summary>
    public partial class PageDeveloper : ModernWpf.Controls.Page
    {
        private bool isInitialize = false;

        public async void MainCycle()
        {
            while (true)
            {
                await Task.Delay(1000);
                //
                string text = "";
                Type type = typeof(AppInfo);

                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (FieldInfo field in fields)
                {
                    string fieldName = field.Name;
                    object? value = field.GetValue(null); // 静态字段传 null
                    text = $"{text}{fieldName} = {value}\n";
                }

                textBlock_varInfo.Text = text;
            }
        }


        public PageDeveloper()
        {
            InitializeComponent();
            MainCycle();
            Loaded += (async (s, e) =>
            {
                await webView2.EnsureCoreWebView2Async(null);

                isInitialize = true;
                new NotificationManager().Show(new NotificationContent
                {
                    Title = "WebView2",
                    Message = "完成初始化",
                    Type = NotificationType.Success
                });
            });
        }

        private void textBox_markdown_TextChanged(object sender, TextChangedEventArgs e)
        {
            markdownViewer.Markdown = textBox_markdown.Text;
        }

        private void textBox_WebView_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInitialize)
            {
                webView2.CoreWebView2.NavigateToString(textBox_WebView.Text);

                /*if (webView2.CoreWebView2 != null)
                    webView2.CoreWebView2.NavigateToString(textBox_WebView.Text);
                else
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "WebView2",
                        Message = "核心未完成初始化",
                        Type = NotificationType.Error
                    });*/
            }
        }
    }
}
