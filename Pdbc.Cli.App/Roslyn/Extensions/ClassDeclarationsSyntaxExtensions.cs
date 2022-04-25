using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Roslyn.Extensions
{
    public static class ClassDeclarationsSyntaxExtensions
    {
        public static PropertyDeclarationSyntax FindPropertyDeclarationSyntaxFor(this ClassDeclarationSyntax classDeclarationSyntax, string name)
        {
            foreach (var property in classDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>())
            {
                var typeSyntax = property.Type.ToFullString();
                if (property.Identifier.ValueText == name)
                    return property;
            }

            return null;
        }

        public static MethodDeclarationSyntax FindMethodDeclarationSyntaxFor(this ClassDeclarationSyntax classDeclarationSyntax, string name)
        {
            foreach (var method in classDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.Identifier.ValueText == name)
                    return method;
            }

            return null;
        }

        public static ClassDeclarationSyntax AddAndKeep(this ClassDeclarationSyntax classDeclarationSyntax, MemberDeclarationSyntax memberDeclarationSyntax)
        {
            var members = classDeclarationSyntax.Members.Add(memberDeclarationSyntax);
            return classDeclarationSyntax.WithMembers(members);
        }

        public static ClassDeclarationSyntax GetClassDeclarationSyntaxFrom(this CompilationUnitSyntax compilationUnitSyntax)
        {
            var namespaceUnitSyntax = compilationUnitSyntax.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            return namespaceUnitSyntax.Members.OfType<ClassDeclarationSyntax>().FirstOrDefault();
        }
    }
}