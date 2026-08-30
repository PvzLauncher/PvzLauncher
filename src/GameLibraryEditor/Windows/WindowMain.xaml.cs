using GameLibraryEditor.JsonConfigs;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace GameLibraryEditor.Windows
{
    /// <summary>
    /// WindowMain.xaml 的交互逻辑
    /// </summary>
    public partial class WindowMain : Window
    {
        public JsonGameIndex.Root GameIndex;
        public JsonGameIndex.GameInfo CurrentGame;


        private void SaveConfig()
        {
            CurrentGame.LastUpdate = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.WriteAllText(Path.Combine(Globals.Config.IndexDirectory, "index.json"), JsonConvert.SerializeObject(GameIndex));
        }

        public WindowMain()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                GameIndex = JsonConvert.DeserializeObject<JsonGameIndex.Root>(File.ReadAllText(Path.Combine(Globals.Config.IndexDirectory, "index.json")))!;

                comboBox_typeSelector.SelectedIndex = 0;
            };


            comboBox_typeSelector.SelectionChanged += (s, e) =>
            {
                listBox_games.Items.Clear();
                JsonGameIndex.GameInfo[] targetType = new JsonGameIndex.GameInfo[0];
                switch (comboBox_typeSelector.SelectedIndex)
                {
                    case 0:
                        targetType = GameIndex!.EnOrigin; break;
                    case 1:
                        targetType = GameIndex!.EnRevision; break;
                    case 2:
                        targetType = GameIndex!.ZhOrigin; break;
                    case 3:
                        targetType = GameIndex!.ZhRevision; break;
                    case 4:
                        targetType = GameIndex!.Trainer; break;
                    case 5:
                        targetType = GameIndex!.Other; break;
                }
                foreach (var game in targetType)
                {
                    var buttonNew = new Button
                    {
                        Content = "+",
                        MinWidth = 20,
                        Tag = game
                    };
                    var buttonRemove = new Button
                    {
                        Content = "-",
                        MinWidth = 20,
                        Tag = game
                    };
                    var buttonUp = new Button
                    {
                        Content = "↑",
                        MinWidth = 20,
                        Tag = game
                    };
                    var buttonDown = new Button
                    {
                        Content = "↓",
                        MinWidth = 20,
                        Tag = game
                    };
                    var item = new ListBoxItem
                    {
                        Content = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                buttonNew,buttonRemove,buttonUp,buttonDown,
                                new TextBlock
                                {
                                    Margin=new Thickness(5,0,0,0),
                                    Text=game.Name
                                }
                            }
                        },
                        Tag = game
                    };
                    listBox_games.Items.Add(item);
                }
            };


            listBox_games.SelectionChanged += (s, e) =>
            {
                if (listBox_games.SelectedItem is not ListBoxItem li || li.Tag is not JsonGameIndex.GameInfo gi)
                    return;
                CurrentGame = gi;

                //===
                textBox_title.Text = gi.Name;
                //===
                var sb = new StringBuilder();
                foreach (var line in gi.Descriptions)
                    sb.AppendLine(line);
                textBox_description.Text = sb.ToString().Trim();
                //===
                listbox_authors.Items.Clear();
                foreach (var au in gi.Authors)
                {
                    var buttonAdd = new Button
                    {
                        Content = "+",
                        MinWidth = 20,
                        Tag = au
                    };
                    var buttonRemove = new Button
                    {
                        Content = "-",
                        MinWidth = 20,
                        Tag = au
                    };
                    var textBoxName = new TextBox
                    {
                        Text = au.Key,
                        MinWidth = 100,
                        Tag = au
                    };
                    var textBoxUrl = new TextBox
                    {
                        Text = au.Value,
                        MinWidth = 100,
                        Tag = au
                    };
                    var item = new ListBoxItem
                    {
                        Tag = au,
                        Content = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                buttonAdd,buttonRemove,textBoxName,textBoxUrl
                            }
                        }
                    };
                    listbox_authors.Items.Add(item);
                    textBoxName.TextChanged += (s, e) =>
                    {
                        if (textBoxName.Tag is not KeyValuePair<string, string> cau) 
                            return;

                        CurrentGame.Authors[textBoxName.Text] = CurrentGame.Authors[cau.Key];
                        CurrentGame.Authors.Remove(cau.Key);
                        textBoxName.Tag = new KeyValuePair<string, string>(textBoxName.Text, cau.Value);
                        textBoxUrl.Tag = textBoxName.Tag;

                        SaveConfig();
                    };
                    textBoxUrl.TextChanged += (s, e) =>
                    {
                        if (textBoxUrl.Tag is not KeyValuePair<string, string> cau)
                            return;

                        CurrentGame.Authors[cau.Key] = textBoxUrl.Text;
                        textBoxUrl.Tag = new KeyValuePair<string, string>(cau.Key, textBoxUrl.Text);
                        textBoxName.Tag = textBoxUrl.Tag;

                        SaveConfig();
                    };
                    buttonAdd.Click += (s, e) =>
                    {
                        CurrentGame.Authors.Add("Name", "Url");
                    };
                }
                //===
                textBox_icon.Text = gi.Icon;
                //===
                textBox_version.Text = gi.Version;
                //===
                textBox_size.Text = gi.Size.ToString();
                //===
                textBox_executeName.Text = gi.ExecuteName;
                //===
                checkBox_recommend.IsChecked = gi.IsRecommend;
                //===
                checkBox_new.IsChecked = gi.IsNew;
                //===
                textBox_shareUrl.Text = gi.ShareUrl;
                //===
                textBox_sharePassword.Text = gi.SharePassword;
                //===
                if (gi.LinkUrls != null)
                {
                    listbox_links.Items.Clear();
                    foreach (var link in gi.LinkUrls)
                    {
                        var buttonAdd = new Button
                        {
                            Content = "+",
                            MinWidth = 20,
                            Tag = link
                        };
                        var buttonRemove = new Button
                        {
                            Content = "-",
                            MinWidth = 20,
                            Tag = link
                        };
                        var textBoxName = new TextBox
                        {
                            Text = link.Key,
                            MinWidth = 100,
                            Tag = link
                        };
                        var textBoxUrl = new TextBox
                        {
                            Text = link.Value,
                            MinWidth = 100,
                            Tag = link
                        };
                        var item = new ListBoxItem
                        {
                            Tag = link,
                            Content = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Children =
                            {
                                buttonAdd,buttonRemove,textBoxName,textBoxUrl
                            }
                            }
                        };
                        listbox_links.Items.Add(item);
                    }
                }
            };


            //========================
            textBox_title.TextChanged += (s, e) =>
            {
                CurrentGame?.Name = textBox_title.Text;
                SaveConfig();
            };
            textBox_description.TextChanged += (s, e) =>
            {
                var descps = new List<string>();
                foreach (var line in textBox_description.Text.Replace("\r", "").Split('\n'))
                    descps.Add(line);
                CurrentGame?.Descriptions = descps.ToArray();
                SaveConfig();
            };
            
        }
    }
}
