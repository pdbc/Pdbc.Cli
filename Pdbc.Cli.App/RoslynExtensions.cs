using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App
{
    public static class RoslynExtensions
    {
        // Move to extension method ??
        public static NamespaceDeclarationSyntax AddUsingStatements(this NamespaceDeclarationSyntax @namespace, params string[] statements)
        {
            foreach (var s in statements)
            {
                @namespace = @namespace.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(s)));
            }

            return @namespace;
        }

        public static ClassDeclarationSyntax AddBaseClasses(this ClassDeclarationSyntax syntax,
            params string[] baseClasses)
        {
            foreach (var s in baseClasses)
            {
                syntax = syntax.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(s)));
            }

            return syntax;
        }
        public static ClassDeclarationSyntax AsPublic(this ClassDeclarationSyntax syntax)
        {
            return syntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }
        public static ClassDeclarationSyntax AddAttribute(this ClassDeclarationSyntax syntax, string attributeName)
        {
            var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName)))
            ).NormalizeWhitespace();

            return syntax.AddAttributeLists(attributeList);
        }
        public static InterfaceDeclarationSyntax AddBaseClasses(this InterfaceDeclarationSyntax syntax,
            params string[] baseClasses)
        {
            foreach (var s in baseClasses)
            {
                syntax = syntax.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(s)));
            }

            return syntax;
        }

        public static MethodDeclarationSyntax AddMethodParameter(this MethodDeclarationSyntax classDeclarationSyntax, 
            string parameterType,
            String parameterName)
        {
            return classDeclarationSyntax
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                    .WithType(SyntaxFactory.ParseTypeName(parameterType)));
        }

        public static MethodDeclarationSyntax AddMethodBody(this MethodDeclarationSyntax classDeclarationSyntax,
            string body)
        {
            var methodBody = SyntaxFactory.ParseStatement(body);
            return classDeclarationSyntax
                .WithBody(SyntaxFactory.Block(methodBody));
            ;
        }
        public static MethodDeclarationSyntax AddAttribute(this MethodDeclarationSyntax syntax, string attributeName)
        {
            var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName)))
            ).NormalizeWhitespace();

            return syntax.AddAttributeLists(attributeList);
        }
        public static InterfaceDeclarationSyntax AsPublic(this InterfaceDeclarationSyntax interfaceDeclarationSyntax)
        {
            return interfaceDeclarationSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }
    }
}