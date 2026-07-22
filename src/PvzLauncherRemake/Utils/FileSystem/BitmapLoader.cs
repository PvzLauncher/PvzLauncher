using System.IO;
using System.Windows.Media.Imaging;

namespace PvzLauncherRemake.Utils.FileSystem
{
    public static class BitmapLoader
    {
        public static BitmapImage LoadBitmapImageFromDisk(string path)
        {
            var bitmap = new BitmapImage();

            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            bitmap.Freeze();

            return bitmap;
        }
    }
}
