using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Extensions
{
    public static class MethodDeclarationSyntaxExtensions
    {
        public static MethodDeclarationSyntax AddMethodBody(this MethodDeclarationSyntax classDeclarationSyntax,
            string body)
        {
            var methodBody = ParseStatement(body);
            return classDeclarationSyntax
                .WithBody(Block(methodBody));
            ;
        }
    }
}