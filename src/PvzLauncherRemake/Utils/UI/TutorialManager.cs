using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Windows;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace PvzLauncherRemake.Utils.UI
{
    public static class TutorialManager
    {
        public static UserTutorialHost _tutorialHost;

        public static void Initialize(UserTutorialHost tutorialHost) => _tutorialHost = tutorialHost;


        public static void ShowTutorial() => _tutorialHost.SetVisible(true);
        public static void HideTutorial() => _tutorialHost.SetVisible(false);
        public static void SetTutorial(string title, string content, Rect target)
        {
            _tutorialHost.Title = title;_tutorialHost.Text = content;
            _tutorialHost.SetHole(target);
        }
        public static void SetTutorial(string title, string content,UIElement target)
        {
            _tutorialHost.Title = title; _tutorialHost.Text = content;
            _tutorialHost.SetHole(target);
        }
    }
}
