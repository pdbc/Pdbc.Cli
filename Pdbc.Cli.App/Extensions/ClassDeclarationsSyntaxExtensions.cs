using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Extensions
{
    public static class ClassDeclarationsSyntaxExtensions
    {
        public static InterfaceDeclarationSyntax AddAndKeep(this InterfaceDeclarationSyntax syntax, MemberDeclarationSyntax memberDeclarationSyntax)
        {
            var members = syntax.Members.Add(memberDeclarationSyntax);
            return syntax.WithMembers(members);
        }
    }
}