using ModernWpf.Controls;
using Newtonsoft.Json;
using PvzLauncherRemake.Classes;
using PvzLauncherRemake.Utils.UI;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using static PvzLauncherRemake.Classes.AppLogger;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageDeveloper.xaml 的交互逻辑
    /// </summary>
    public partial class PageDeveloper : ModernWpf.Controls.Page
    {
        public void Initialize()
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
