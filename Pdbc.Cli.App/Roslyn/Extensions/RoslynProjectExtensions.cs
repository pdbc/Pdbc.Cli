using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Visitors;

namespace Pdbc.Cli.App.Roslyn.Extensions
{
    public static class RoslynProjectContextExtensions
    {
        public static async Task<List<ClassDeclarationSyntax>> GetclassesFromProject(this RoslynProjectContext roslynProjectContext, bool forceReload = false)
        {
            if (!forceReload) {
                if (roslynProjectContext.Classes != null)
                    return roslynProjectContext.Classes;
            }

            var compilation = await roslynProjectContext.Project.GetCompilationAsync();

            var classVisitor = new ClassVirtualizationVisitor();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                classVisitor.Visit(syntaxTree.GetRoot());
            }

            roslynProjectContext.Classes = classVisitor.Classes;
            return roslynProjectContext.Classes;
        }

        public static async Task<ClassDeclarationSyntax> GetClassByName(this RoslynProjectContext roslynProjectContext, string className)
        {
            var classes = await roslynProjectContext.GetclassesFromProject();
            var entity = classes.FirstOrDefault(x => x.Identifier.ValueText == className);
            if (entity != null)
            {
                return entity;
            }

            return null;
        }
        public static async Task<ClassDeclarationSyntax> GetClassEndingWithName(this RoslynProjectContext roslynProjectContext, string className)
        {
            var classes = await roslynProjectContext.GetclassesFromProject();
            var entity = classes.FirstOrDefault(x => x.Identifier.ValueText.EndsWith(className));
            if (entity != null)
            {
                return entity;
            }

            return null;
        }
    }
}