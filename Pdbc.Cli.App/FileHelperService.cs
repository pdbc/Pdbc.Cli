using System;
using System.IO;
using System.Linq;

namespace Pdbc.Cli.App
{
    public class FileHelperService
    {
        public String GetSolutionPathFrom(string path)
        {
            string[] files = Directory.GetFiles(path, "*.sln");
            return files.FirstOrDefault();
        }

        public void WriteFile(String path, String className, String content)
        {
            var filePath = Path.Combine(path, $"{className}.cs");
            File.WriteAllTextAsync(filePath, content);
        }
    }
}