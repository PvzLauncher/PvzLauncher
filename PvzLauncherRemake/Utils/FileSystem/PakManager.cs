using System.IO;
using System.Text;

namespace PvzLauncherRemake.Utils.FileSystem
{
    public class PakManager
    {
        /// <summary>
        /// 解包 main.pak 文件到指定输出目录
        /// </summary>
        /// <param name="pakPath">main.pak 文件的完整路径</param>
        /// <param name="outputDirectory">输出目录（会自动创建）</param>
        /// <exception cref="ArgumentException">参数无效时抛出</exception>
        /// <exception cref="InvalidDataException">PAK 文件格式无效时抛出</exception>
        /// <exception cref="IOException">读取或写入文件失败时抛出</exception>
        public static void Unpack(string pakPath, string outputDirectory)
        {
            if (string.IsNullOrWhiteSpace(pakPath))
                throw new ArgumentException("PAK 文件路径不能为空。", nameof(pakPath));
            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentException("输出目录不能为空。", nameof(outputDirectory));
            if (!File.Exists(pakPath))
                throw new FileNotFoundException("找不到指定的 main.pak 文件。", pakPath);

            // 读取并整体 XOR 解密（密钥 0xF7）
            byte[] data = File.ReadAllBytes(pakPath);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= 0xF7;
            }

            using MemoryStream ms = new MemoryStream(data);
            using BinaryReader br = new BinaryReader(ms, Encoding.UTF8);

            // 验证魔数
            uint magic = br.ReadUInt32();
            uint version = br.ReadUInt32(); // 通常为 0，可忽略
            if (magic != 0xBAC04AC0)
                throw new InvalidDataException("无效的 PAK 文件魔数（应为 0xBAC04AC0）。");

            // 存储文件条目信息
            var entries = new List<(string Name, int Size)>();

            while (true)
            {
                byte mark = br.ReadByte();
                if (mark == 0x80) // 结束标记
                    break;
                if (mark != 0x00)
                    throw new InvalidDataException($"无效的条目标记：0x{mark:X2}");

                byte nameLength = br.ReadByte();
                byte[] nameBytes = br.ReadBytes(nameLength);
                string fileName = Encoding.UTF8.GetString(nameBytes);

                uint fileSize = br.ReadUInt32();
                if (fileSize > int.MaxValue)
                    throw new InvalidDataException("单个文件大小超过支持范围。");

                br.ReadBytes(8); // 跳过 8 字节时间戳（FILETIME）

                entries.Add((fileName, (int)fileSize));
            }

            // 创建输出根目录
            Directory.CreateDirectory(outputDirectory);

            // 逐个提取文件
            foreach (var (fileName, size) in entries)
            {
                // 统一路径分隔符为 Windows 风格（原 PAK 中可能使用 '/'）
                string normalizedPath = fileName.Replace('/', '\\');
                string fullPath = Path.Combine(outputDirectory, normalizedPath);

                // 创建子目录
                string? directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                // 写入文件数据
                byte[] fileData = br.ReadBytes(size);
                File.WriteAllBytes(fullPath, fileData);
            }
        }

        /// <summary>
        /// 将指定文件夹打包为 main.pak 文件
        /// </summary>
        /// <param name="inputDirectory">输入目录（解包后的资源根目录）</param>
        /// <param name="pakPath">输出的 main.pak 文件完整路径（会覆盖现有文件）</param>
        /// <exception cref="ArgumentException">参数无效时抛出</exception>
        /// <exception cref="IOException">读取或写入文件失败时抛出</exception>
        public static void Pack(string inputDirectory, string pakPath)
        {
            if (string.IsNullOrWhiteSpace(inputDirectory))
                throw new ArgumentException("输入目录不能为空。", nameof(inputDirectory));
            if (string.IsNullOrWhiteSpace(pakPath))
                throw new ArgumentException("PAK 文件路径不能为空。", nameof(pakPath));
            if (!Directory.Exists(inputDirectory))
                throw new DirectoryNotFoundException("指定的输入目录不存在。");

            // 收集所有文件及其相对路径
            var files = new List<(string RelativePath, string FullPath)>();
            foreach (string fullPath in Directory.EnumerateFiles(inputDirectory, "*.*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(inputDirectory, fullPath);
                // PAK 内部统一使用 '/' 作为路径分隔符
                string pakInternalPath = relativePath.Replace('\\', '/');
                files.Add((pakInternalPath, fullPath));
            }

            // 按文件名排序（与原版保持一致，提高兼容性）
            files.Sort((a, b) => string.Compare(a.RelativePath, b.RelativePath, StringComparison.OrdinalIgnoreCase));

            using MemoryStream ms = new MemoryStream();
            using BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8);

            // 写入头部
            bw.Write(0xBAC04AC0u); // 魔数（小端序）
            bw.Write(0u);          // 版本（通常为 0）

            // 写入文件条目
            foreach (var (relativePath, fullPath) in files)
            {
                byte[] nameBytes = Encoding.UTF8.GetBytes(relativePath);

                if (nameBytes.Length > 255)
                    throw new InvalidDataException($"文件名过长（超过 255 字节）：{relativePath}");

                bw.Write((byte)0x00);               // 条目标记 0x00
                bw.Write((byte)nameBytes.Length);   // 名称长度
                bw.Write(nameBytes);                // 文件名（UTF-8）
                uint fileSize = (uint)new FileInfo(fullPath).Length;
                bw.Write(fileSize);                 // 文件大小
                bw.Write(new byte[8]);              // 时间戳占位（全 0）
            }

            // 写入结束标记
            bw.Write((byte)0x80);

            // 写入所有文件数据
            foreach (var (_, fullPath) in files)
            {
                byte[] fileData = File.ReadAllBytes(fullPath);
                bw.Write(fileData);
            }

            // 整体 XOR 加密（密钥 0xF7）
            byte[] packedData = ms.ToArray();
            for (int i = 0; i < packedData.Length; i++)
            {
                packedData[i] ^= 0xF7;
            }

            // 写入最终 PAK 文件
            File.WriteAllBytes(pakPath, packedData);
        }
    }
}
