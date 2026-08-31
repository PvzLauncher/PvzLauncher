using GameLibraryEditor.JsonConfigs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            File.WriteAllText(Path.Combine(Globals.Config.IndexDirectory, "index.json"), JsonConvert.SerializeObject(GameIndex, Formatting.Indented));
        }

        private JsonGameIndex.GameInfo[] GetCurrentTypeArray()
        {
            return comboBox_typeSelector.SelectedIndex switch
            {
                0 => GameIndex.EnOrigin,
                1 => GameIndex.EnRevision,
                2 => GameIndex.ZhOrigin,
                3 => GameIndex.ZhRevision,
                4 => GameIndex.Trainer.Cast<JsonGameIndex.GameInfo>().ToArray(),
                5 => GameIndex.Other,
                _ => Array.Empty<JsonGameIndex.GameInfo>()
            };
        }

        private void SetCurrentTypeArray(JsonGameIndex.GameInfo[] array)
        {
            switch (comboBox_typeSelector.SelectedIndex)
            {
                case 0: GameIndex.EnOrigin = array; break;
                case 1: GameIndex.EnRevision = array; break;
                case 2: GameIndex.ZhOrigin = array; break;
                case 3: GameIndex.ZhRevision = array; break;
                case 4: GameIndex.Trainer = array.OfType<JsonGameIndex.TrainerInfo>().ToArray(); break;
                case 5: GameIndex.Other = array; break;
            }
        }

        private void RefreshGameList()
        {
            var idx = comboBox_typeSelector.SelectedIndex;
            comboBox_typeSelector.SelectedIndex = -1;
            comboBox_typeSelector.SelectedIndex = idx;
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
                var targetType = GetCurrentTypeArray();

                foreach (var game in targetType)
                {
                    var buttonNew = new Button { Content = "+", MinWidth = 20, Tag = game };
                    var buttonRemove = new Button { Content = "-", MinWidth = 20, Tag = game };
                    var buttonUp = new Button { Content = "↑", MinWidth = 20, Tag = game };
                    var buttonDown = new Button { Content = "↓", MinWidth = 20, Tag = game };

                    buttonNew.Click += (bs, be) =>
                    {
                        var list = GetCurrentTypeArray().ToList();
                        var newGame = comboBox_typeSelector.SelectedIndex == 4
                            ? (JsonGameIndex.GameInfo)new JsonGameIndex.TrainerInfo
                            {
                                Name = "New Trainer",
                                Descriptions = Array.Empty<string>(),
                                Authors = new Dictionary<string, string>(),
                                LinkUrls = new Dictionary<string, string>(),
                                SupportVersion = ""
                            }
                            : new JsonGameIndex.GameInfo
                            {
                                Name = "New Game",
                                Descriptions = Array.Empty<string>(),
                                Authors = new Dictionary<string, string>(),
                                LinkUrls = new Dictionary<string, string>()
                            };
                        list.Add(newGame);
                        SetCurrentTypeArray(list.ToArray());
                        SaveConfig();
                        RefreshGameList();
                    };

                    buttonRemove.Click += (bs, be) =>
                    {
                        var list = GetCurrentTypeArray().ToList();
                        list.Remove(game);
                        SetCurrentTypeArray(list.ToArray());
                        CurrentGame = null;
                        SaveConfig();
                        RefreshGameList();
                    };

                    buttonUp.Click += (bs, be) =>
                    {
                        var list = GetCurrentTypeArray().ToList();
                        int index = list.IndexOf(game);
                        if (index <= 0) return;
                        list.RemoveAt(index);
                        list.Insert(index - 1, game);
                        SetCurrentTypeArray(list.ToArray());
                        SaveConfig();
                        RefreshGameList();
                    };

                    buttonDown.Click += (bs, be) =>
                    {
                        var list = GetCurrentTypeArray().ToList();
                        int index = list.IndexOf(game);
                        if (index >= list.Count - 1) return;
                        list.RemoveAt(index);
                        list.Insert(index + 1, game);
                        SetCurrentTypeArray(list.ToArray());
                        SaveConfig();
                        RefreshGameList();
                    };

                    var item = new ListBoxItem
                    {
                        Content = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                buttonNew, buttonRemove, buttonUp, buttonDown,
                                new TextBlock { Margin = new Thickness(5, 0, 0, 0), Text = game.Name, VerticalAlignment = VerticalAlignment.Center }
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

                textBox_title.Text = gi.Name;

                var sb = new StringBuilder();
                foreach (var line in gi.Descriptions)
                    sb.AppendLine(line);
                textBox_description.Text = sb.ToString().TrimEnd();

                listbox_authors.Items.Clear();
                foreach (var au in gi.Authors)
                    AddAuthorItem(au);

                textBox_icon.Text = gi.Icon;
                textBox_version.Text = gi.Version;
                textBox_size.Text = gi.Size.ToString();
                textBox_executeName.Text = gi.ExecuteName;
                checkBox_recommend.IsChecked = gi.IsRecommend;
                checkBox_new.IsChecked = gi.IsNew;
                textBox_shareUrl.Text = gi.ShareUrl;
                textBox_sharePassword.Text = gi.SharePassword;

                listbox_links.Items.Clear();
                if (gi.LinkUrls != null)
                    foreach (var link in gi.LinkUrls)
                        AddLinkItem(link);
            };

            textBox_title.TextChanged += (s, e) =>
            {
                CurrentGame.Name = textBox_title.Text;
                if (listBox_games.SelectedItem is ListBoxItem selectedItem &&
                    selectedItem.Content is StackPanel sp &&
                    sp.Children.OfType<TextBlock>().FirstOrDefault() is TextBlock tb)
                    tb.Text = textBox_title.Text;
                SaveConfig();
            };

            textBox_description.TextChanged += (s, e) =>
            {
                CurrentGame.Descriptions = textBox_description.Text.Replace("\r", "").Split('\n');
                SaveConfig();
            };

            textBox_icon.TextChanged += (s, e) =>
            {
                CurrentGame.Icon = textBox_icon.Text;
                SaveConfig();
            };

            textBox_version.TextChanged += (s, e) =>
            {
                CurrentGame.Version = textBox_version.Text;
                SaveConfig();
            };

            textBox_size.TextChanged += (s, e) =>
            {
                CurrentGame.Size = double.Parse(textBox_size.Text);
                SaveConfig();
            };

            textBox_executeName.TextChanged += (s, e) =>
            {
                CurrentGame.ExecuteName = textBox_executeName.Text;
                SaveConfig();
            };

            checkBox_recommend.Checked += (s, e) => { CurrentGame.IsRecommend = true; SaveConfig(); };
            checkBox_recommend.Unchecked += (s, e) => { CurrentGame.IsRecommend = false; SaveConfig(); };

            checkBox_new.Checked += (s, e) => { CurrentGame.IsNew = true; SaveConfig(); };
            checkBox_new.Unchecked += (s, e) => { CurrentGame.IsNew = false; SaveConfig(); };

            textBox_shareUrl.TextChanged += (s, e) =>
            {
                CurrentGame.ShareUrl = textBox_shareUrl.Text;
                SaveConfig();
            };

            textBox_sharePassword.TextChanged += (s, e) =>
            {
                CurrentGame.SharePassword = textBox_sharePassword.Text;
                SaveConfig();
            };
        }

        private void AddAuthorItem(KeyValuePair<string, string> au)
        {
            var buttonAdd = new Button { Content = "+", MinWidth = 20 };
            var buttonRemove = new Button { Content = "-", MinWidth = 20 };
            var textBoxName = new TextBox { Text = au.Key, MinWidth = 100, Tag = au };
            var textBoxUrl = new TextBox { Text = au.Value, MinWidth = 100, Tag = au };

            var item = new ListBoxItem
            {
                Tag = au,
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { buttonAdd, buttonRemove, textBoxName, textBoxUrl }
                }
            };

            textBoxName.TextChanged += (s, e) =>
            {
                var oldKv = (KeyValuePair<string, string>)textBoxName.Tag;
                string value = CurrentGame.Authors[oldKv.Key];
                CurrentGame.Authors.Remove(oldKv.Key);
                CurrentGame.Authors[textBoxName.Text] = value;
                var newKv = new KeyValuePair<string, string>(textBoxName.Text, value);
                textBoxName.Tag = newKv;
                textBoxUrl.Tag = newKv;
                item.Tag = newKv;
                SaveConfig();
            };

            textBoxUrl.TextChanged += (s, e) =>
            {
                var oldKv = (KeyValuePair<string, string>)textBoxUrl.Tag;
                CurrentGame.Authors[oldKv.Key] = textBoxUrl.Text;
                var newKv = new KeyValuePair<string, string>(oldKv.Key, textBoxUrl.Text);
                textBoxUrl.Tag = newKv;
                textBoxName.Tag = newKv;
                item.Tag = newKv;
                SaveConfig();
            };

            buttonAdd.Click += (s, e) =>
            {
                CurrentGame.Authors.Add("Name", "Url");
                SaveConfig();
                listBox_games.SelectedItem = listBox_games.SelectedItem;
            };

            buttonRemove.Click += (s, e) =>
            {
                var kv = (KeyValuePair<string, string>)textBoxName.Tag;
                CurrentGame.Authors.Remove(kv.Key);
                listbox_authors.Items.Remove(item);
                SaveConfig();
            };

            listbox_authors.Items.Add(item);
        }

        private void AddLinkItem(KeyValuePair<string, string> link)
        {
            var buttonAdd = new Button { Content = "+", MinWidth = 20 };
            var buttonRemove = new Button { Content = "-", MinWidth = 20 };
            var textBoxName = new TextBox { Text = link.Key, MinWidth = 100, Tag = link };
            var textBoxUrl = new TextBox { Text = link.Value, MinWidth = 100, Tag = link };

            var item = new ListBoxItem
            {
                Tag = link,
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { buttonAdd, buttonRemove, textBoxName, textBoxUrl }
                }
            };

            textBoxName.TextChanged += (s, e) =>
            {
                var oldKv = (KeyValuePair<string, string>)textBoxName.Tag;
                string value = CurrentGame.LinkUrls[oldKv.Key];
                CurrentGame.LinkUrls.Remove(oldKv.Key);
                CurrentGame.LinkUrls[textBoxName.Text] = value;
                var newKv = new KeyValuePair<string, string>(textBoxName.Text, value);
                textBoxName.Tag = newKv;
                textBoxUrl.Tag = newKv;
                item.Tag = newKv;
                SaveConfig();
            };

            textBoxUrl.TextChanged += (s, e) =>
            {
                var oldKv = (KeyValuePair<string, string>)textBoxUrl.Tag;
                CurrentGame.LinkUrls[oldKv.Key] = textBoxUrl.Text;
                var newKv = new KeyValuePair<string, string>(oldKv.Key, textBoxUrl.Text);
                textBoxUrl.Tag = newKv;
                textBoxName.Tag = newKv;
                item.Tag = newKv;
                SaveConfig();
            };

            buttonAdd.Click += (s, e) =>
            {
                CurrentGame.LinkUrls.Add("Link", "Url");
                SaveConfig();
                listBox_games.SelectedItem = listBox_games.SelectedItem;
            };

            buttonRemove.Click += (s, e) =>
            {
                var kv = (KeyValuePair<string, string>)textBoxName.Tag;
                CurrentGame.LinkUrls.Remove(kv.Key);
                listbox_links.Items.Remove(item);
                SaveConfig();
            };

            listbox_links.Items.Add(item);
        }
    }
}