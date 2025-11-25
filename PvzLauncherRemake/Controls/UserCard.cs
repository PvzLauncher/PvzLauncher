using System.Windows;
using System.Windows.Controls;

namespace PvzLauncherRemake.Controls
{
    public class UserCard:ContentControl
    {
        static UserCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UserCard), new FrameworkPropertyMetadata(typeof(UserCard)));
        }
    }
}
