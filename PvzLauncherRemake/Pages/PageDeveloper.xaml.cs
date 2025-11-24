using MdXaml;
using PvzLauncherRemake.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// PageDeveloper.xaml 的交互逻辑
    /// </summary>
    public partial class PageDeveloper : ModernWpf.Controls.Page
    {
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
        }

        private void textBox_markdown_TextChanged(object sender, TextChangedEventArgs e)
        {
            markdownViewer.Markdown = textBox_markdown.Text;
        }
    }
}
