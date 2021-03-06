using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Extensions
{
    public static class CompilationUnitSyntaxExtensions
    {
        public static CompilationUnitSyntax AddUsingStatements(this CompilationUnitSyntax @namespace, params string[] statements)
        {
            foreach (var s in statements)
            {
                @namespace = @namespace.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(s)));
            }

            return @namespace;
        }

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


        public static TSyntax GetSyntaxNodeFrom<TSyntax>(this CompilationUnitSyntax compilationUnitSyntax) where TSyntax : SyntaxNode
        {
            var namespaceUnitSyntax = compilationUnitSyntax.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            return namespaceUnitSyntax.Members.OfType<TSyntax>().FirstOrDefault();
        }
    }
}