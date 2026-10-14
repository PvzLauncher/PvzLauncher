using System.IO;
using System.IO.Compression;

namespace PvzLauncherRemake.Utils.FileSystem
{
    public static class GzipHelper
    {
        /// <summary>
        /// 对文件进行Gzip压缩
        /// </summary>
        /// <param name="inputPath">文件输入</param>
        /// <param name="outputPath">文件输出</param>
        public static void Compress(string inputPath, string outputPath)
        {
            using var input = File.OpenRead(inputPath);
            using var output = File.Create(outputPath);
            using var gzip = new GZipStream(output, CompressionLevel.SmallestSize);
            input.CopyTo(gzip);
        }

        /// <summary>
        /// 对文件进行Gzip解压
        /// </summary>
        /// <param name="inputPath">文件输入</param>
        /// <param name="outputPath">文件输出</param>
        public static void Decompress(string inputPath,string outputPath)
        {
            using var input = File.OpenRead(inputPath);
            using var output = File.Create(outputPath);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            gzip.CopyTo(output);
        }
    }
}
