using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Roslyn.Extensions
{
    public static class TypeDeclarationSyntaxExtensions
    {
        //public static FieldDeclarationSyntax FindVariableDeclarationSyntaxFor(this TypeDeclarationSyntax typeSyntax, string name)
        //{
        //    foreach (var property in typeSyntax.Members.OfType<FieldDeclarationSyntax>())
        //    {

        //        foreach (var v in property.Declaration.Variables)
        //        {
        //            if (v.Identifier.ValueText == name)
        //                return property;
        //        }
        //    }

        //    return null;
        //}
        public static ConstructorDeclarationSyntax FindConstructorDeclarationSyntax(this TypeDeclarationSyntax typeSyntax)
        {
            foreach (var method in typeSyntax.Members.OfType<ConstructorDeclarationSyntax>())
            {
                // TODO Verify the parameters if this is the constructor we need
                return method;
                //method.ParameterList.For
            }

            return null;
        }

        public static TypeDeclarationSyntax AddAndKeep(this TypeDeclarationSyntax syntax, MemberDeclarationSyntax memberDeclarationSyntax) 
        {
            var members = syntax.Members.Add(memberDeclarationSyntax);
            return syntax.WithMembers(members);
        }

    }
}