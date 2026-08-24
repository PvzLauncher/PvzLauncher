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
        public static UserTutorialHost TutorialHost;

        public static void Initialize(UserTutorialHost tutorialHost) => TutorialHost = tutorialHost;


        public static void ShowTutorial() => TutorialHost.SetVisible(true);
        public static void HideTutorial() => TutorialHost.SetVisible(false);
        public static void SetTutorial(string title, string content, Rect target)
        {
            TutorialHost.Title = title; TutorialHost.Text = content;
            TutorialHost.SetHole(target);
        }
        public static void SetTutorial(string title, string content,UIElement target)
        {
            TutorialHost.Title = title; TutorialHost.Text = content;
            TutorialHost.SetHole(target);
        }
    }
}
