using ModernWpf.Controls;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Utils.UI;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using static PvzLauncherRemake.Class.AppLogger;

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
            logger.Info($"[开发者控制面板:周期循环] 开始进入周期循环");
            while (true)
            {
                await Task.Delay(1000);

                // =====

                string text = "";
                Type type = typeof(AppGlobals);

                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (FieldInfo field in fields)
                {
                    string fieldName = field.Name;
                    object? value = field.GetValue(null);
                    text = $"{text}{fieldName} = {value}\n";
                }

                textBlock_varInfo.Text = text;
            }
        }


        public PageDeveloper()
        {
            InitializeComponent();
            Loaded += (async (s, e) =>
            {
                isInitialize = true;
                logger.Info($"[开发者控制面板] 完成初始化!");
            });
            MainCycle();

        }

        private void textBox_markdown_TextChanged(object sender, TextChangedEventArgs e)
        {
            markdownViewer.Markdown = textBox_markdown.Text;
        }

        private void textBox_Md2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInitialize)
            {
                var markdown = new MdXaml.Markdown();

                FlowDocument doc = markdown.Transform(textBox_Md2.Text);

                doc.FontFamily = new FontFamily("Microsoft Yahei UI");

                foreach (Paragraph p in doc.Blocks.OfType<Paragraph>())
                {
                    p.LineHeight = 10;
                    p.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                }

                flowDocumentScrollViewer_Md2.Document = doc;
            }
        }

        private async void button_Dialog_Show_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                async void ShowDialog(ContentDialog dialog)
                {
                    await DialogManager.ShowDialogAsync(dialog);
                }
                int count = 0;
                switch ((string)button.Tag)
                {
                    case "1": count = 1; break;
                    case "5": count = 5; break;
                    case "10": count = 10; break;
                    case "50": count = 50; break;
                }

                for (int i = 0; i < count; i++)
                {
                    ShowDialog(new ContentDialog
                    {
                        Title = textBox_Dialog_Title.Text,
                        Content = textBox_Dialog_Content.Text,
                        PrimaryButtonText = textBox_Dialog_Button.Text,
                        DefaultButton = ContentDialogButton.Primary
                    });
                }
            }
        }
    }
}
