using System.IO;

namespace ArchiForge.Utils
{
    public static class FileHelper
    {
        public static void WriteAllText(string path, string content)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, content);
        }
    }
}
