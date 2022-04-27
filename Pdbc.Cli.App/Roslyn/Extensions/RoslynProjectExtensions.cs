using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Visitors;

namespace Pdbc.Cli.App.Roslyn.Extensions
{
    public static class RoslynProjectContextExtensions
    {
        public static async Task LoadClassesAndInterfaces(this RoslynProjectContext roslynProjectContext, bool forceReload = false)
        {
            var compilation = await roslynProjectContext.Project.GetCompilationAsync();
            var classVisitor = new ClassVirtualizationVisitor();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                classVisitor.Visit(root);
            }

            roslynProjectContext.Classes = classVisitor.Classes;
            roslynProjectContext.Interfaces = classVisitor.Interfaces;
        }

        public static async Task<ClassDeclarationSyntax> GetClassByName(this RoslynProjectContext roslynProjectContext, string className)
        {
            await roslynProjectContext.LoadClassesAndInterfaces();
            var entity = roslynProjectContext.Classes.FirstOrDefault(x => x.Identifier.ValueText == className);
            if (entity != null)
            {
                return entity;
            }

            return null;
        }
        public static async Task<ClassDeclarationSyntax> GetClassEndingWithName(this RoslynProjectContext roslynProjectContext, string className)
        {
            await roslynProjectContext.LoadClassesAndInterfaces();
            var entity = roslynProjectContext.Classes.FirstOrDefault(x => x.Identifier.ValueText.EndsWith(className));
            if (entity != null)
            {
                return entity;
            }

            return null;
        }
        public static async Task<InterfaceDeclarationSyntax> GetInterfaceByName(this RoslynProjectContext roslynProjectContext, string className)
        {
            await roslynProjectContext.LoadClassesAndInterfaces();
            var entity = roslynProjectContext.Interfaces.FirstOrDefault(x => x.Identifier.ValueText == className);
            if (entity != null)
            {
                return entity;
            }

            return null;
        }
        
    }
}