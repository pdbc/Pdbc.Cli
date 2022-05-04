using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Extensions
{
    public static class CompilationUnitSyntaxExtensions
    {
        public static ClassDeclarationSyntax GetClassDeclarationSyntaxFrom(this CompilationUnitSyntax compilationUnitSyntax)
        {
            var namespaceUnitSyntax = compilationUnitSyntax.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            return namespaceUnitSyntax.Members.OfType<ClassDeclarationSyntax>().FirstOrDefault();
        }

        public static InterfaceDeclarationSyntax GetInterfaceDeclarationSyntaxFrom(this CompilationUnitSyntax compilationUnitSyntax)
        {
            var namespaceUnitSyntax = compilationUnitSyntax.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            return namespaceUnitSyntax.Members.OfType<InterfaceDeclarationSyntax>().FirstOrDefault();
        }
    }
}