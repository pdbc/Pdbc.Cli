using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Roslyn.Extensions
{
    public static class ClassDeclarationsSyntaxExtensions
    {



        //public static MethodDeclarationSyntax FindMethodDeclarationSyntaxFor(this ClassDeclarationSyntax classDeclarationSyntax, string name)
        //{
        //    foreach (var method in classDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>())
        //    {
        //        if (method.Identifier.ValueText == name)
        //            return method;
        //    }

        //    return null;
        //}


        public static ClassDeclarationSyntax AddAndKeep(this ClassDeclarationSyntax syntax, MemberDeclarationSyntax memberDeclarationSyntax)
        {
            var members = syntax.Members.Add(memberDeclarationSyntax);
            return syntax.WithMembers(members);
        }
        public static InterfaceDeclarationSyntax AddAndKeep(this InterfaceDeclarationSyntax syntax, MemberDeclarationSyntax memberDeclarationSyntax)
        {
            var members = syntax.Members.Add(memberDeclarationSyntax);
            return syntax.WithMembers(members);
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
    }
}