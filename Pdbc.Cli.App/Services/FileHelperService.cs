using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Services
{
    public class FileHelperService
    {
        public String GetSolutionPathFrom(string path)
        {
            string[] files = Directory.GetFiles(path, "*.sln");
            return files.FirstOrDefault();
        }

        public async Task<String> WriteFile(String path, String className, String content)
        {
            var filename = Path.Combine(path, $"{className}.cs");
            return await WriteFile(filename, content);
        }
        public async Task<String> WriteFile(String filename,String content)
        {
            await File.WriteAllTextAsync(filename, content);
            return filename;
        }
        public String GetApplicationDirectory()
        {
            var directory = Directory.GetCurrentDirectory();
            Console.WriteLine($"ApplicationDirectory: {directory}");
            return directory;
        }

        public T ReadJsonConfigurationFile<T>(string directory) where T : new()
        {
            var filename = Path.Combine(directory, "cli-configuration.json");
            if (File.Exists(filename))
            {
                var text = File.ReadAllText(filename);
                return JsonSerializer.Deserialize<T>(text);
            }

            return new T();
        }


        public async Task WriteFile(String filename, SyntaxNode syntaxNode)
        {
            var compilationUnitSyntax = syntaxNode.GetParentNodeOfType<CompilationUnitSyntax>();
            var code = compilationUnitSyntax
                .NormalizeWhitespace()
                .ToFullString();

            // write the file to disk
            await WriteFile(filename, code);
            
        }
    }
}