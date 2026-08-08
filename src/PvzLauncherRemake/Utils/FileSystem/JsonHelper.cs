using Newtonsoft.Json;
using System.IO;

namespace PvzLauncherRemake.Utils.FileSystem
{
    public static class JsonHelper
    {
        /// <summary>
        /// 读取Json
        /// </summary>
        /// <param name="content">Json文件路径或Json文本</param>
        /// <returns>反序列化结果</returns>
        public static T ReadJson<T>(string contentOrPath)
        {
            string jsonText;

            // 判断是否是文件
            if (File.Exists(contentOrPath))
            {
                jsonText = File.ReadAllText(contentOrPath);
            }
            else
            {
                jsonText = contentOrPath;
            }

            if (string.IsNullOrWhiteSpace(jsonText))
                throw new Exception("JSON 内容为空");

            try
            {
                return JsonConvert.DeserializeObject<T>(jsonText)!;
            }
            catch (JsonException ex)
            {
                throw new Exception("无效的 JSON 内容", ex);
            }
        }

        /// <summary>
        /// 将对象序列化为JSON并写入文件
        /// </summary>
        public static void WriteJson<T>(string filePath, T data)
        {
            var serializer = new JsonSerializer
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            using (var sw = new StreamWriter(filePath))
            using (var writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Newtonsoft.Json.Formatting.Indented;
                writer.IndentChar = ' ';
                writer.Indentation = 4;
                serializer.Serialize(writer, data);
            }
        }
    }
}
