using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            
            //const string targetPath = @"C:\repos\Pdbc\Pdbc.Shopping\Pdbc.Shopping.sln";
            const string targetPath = @"C:\repos\Development\IM.Scharnier\IM.Scharnier.sln";

            MSBuildLocator.RegisterDefaults();
            var workspace = MSBuildWorkspace.Create();
            var sln = await workspace.OpenSolutionAsync(targetPath);
            
            Project? domainProject = sln.Projects.FirstOrDefault(x => x.Name.Contains("Domain"));
            Project? dataProject = sln.Projects.FirstOrDefault(x => x.Name.Contains("Data"));


            await GenerateEntity(domainProject, "Samsung");
            await GenerateRepository(dataProject, "Samsung");
            await GenerateEntityMappingConfiguration(dataProject, "Samsung");
        }

        private static async Task GenerateEntity(Project? project, string entityName)
        {
            var codeGenerationService = new CodeGeneration();

            var compilation = await project.GetCompilationAsync();
            var classVisitor = new ClassVirtualizationVisitor();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                classVisitor.Visit(syntaxTree.GetRoot());
            }

            
            var modelFilePath = Path.Combine(Path.GetDirectoryName(project.FilePath), "model");
            var classes = classVisitor.Classes;
            ClassDeclarationSyntax? c = classes.FirstOrDefault();

            NamespaceDeclarationSyntax namespaceDeclarationSyntax = null;
            if (!SyntaxNodeHelper.TryGetParentSyntax(c, out namespaceDeclarationSyntax))
            {
                return;
            }

            var index = namespaceDeclarationSyntax.Name.ToString().IndexOf("Domain");
            var namespaceName = namespaceDeclarationSyntax.Name.ToString().Substring(0, index + 6) + ".Model";

            var entity = classes.FirstOrDefault(x => x.Identifier.ValueText == entityName);

            if (entity == null)
            {
                var code = codeGenerationService.CreateEntityClass(namespaceName, entityName);

                await File.WriteAllTextAsync($"{modelFilePath}\\{entityName}.cs", code);
            }
        }

        private static async Task GenerateRepository(Project? project, string entityName)
        {
            var codeGenerationService = new CodeGeneration();

            var compilation = await project.GetCompilationAsync();
            var classVisitor = new ClassVirtualizationVisitor();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                classVisitor.Visit(syntaxTree.GetRoot());
            }


            var modelFilePath = Path.Combine(Path.GetDirectoryName(project.FilePath), "Repositories");
            var classes = classVisitor.Classes;
            ClassDeclarationSyntax? c = classes.FirstOrDefault();

            NamespaceDeclarationSyntax namespaceDeclarationSyntax = null;
            if (!SyntaxNodeHelper.TryGetParentSyntax(c, out namespaceDeclarationSyntax))
            {
                return;
            }

            var index = namespaceDeclarationSyntax.Name.ToString().IndexOf("Data");
            var namespaceName = namespaceDeclarationSyntax.Name.ToString().Substring(0, index + 4) + ".Repositories";

            var entity = classes.FirstOrDefault(x => x.Identifier.ValueText == $"{entityName}Repository");

            if (entity == null)
            {
                var code = codeGenerationService.CreateRepository(namespaceName, entityName);

                await File.WriteAllTextAsync($"{modelFilePath}\\{entityName}Repository.cs", code);
            }
        }

        private static async Task GenerateEntityMappingConfiguration(Project? project, string entityName)
        {
            var codeGenerationService = new CodeGeneration();

            var compilation = await project.GetCompilationAsync();
            var classVisitor = new ClassVirtualizationVisitor();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                classVisitor.Visit(syntaxTree.GetRoot());
            }


            var modelFilePath = Path.Combine(Path.GetDirectoryName(project.FilePath), "Configurations");
            var classes = classVisitor.Classes;
            ClassDeclarationSyntax? c = classes.FirstOrDefault();

            NamespaceDeclarationSyntax namespaceDeclarationSyntax = null;
            if (!SyntaxNodeHelper.TryGetParentSyntax(c, out namespaceDeclarationSyntax))
            {
                return;
            }

            var index = namespaceDeclarationSyntax.Name.ToString().IndexOf("Configurations");
            var namespaceName = namespaceDeclarationSyntax.Name.ToString().Substring(0, index + 14) + ".Configurations";

            var entity = classes.FirstOrDefault(x => x.Identifier.ValueText == $"{entityName}Configuration");

            if (entity == null)
            {
                var code = codeGenerationService.CreateEntityConfiguration(namespaceName, entityName);

                await File.WriteAllTextAsync($"{modelFilePath}\\{entityName}Configuration.cs", code);
            }
        }
    }

}