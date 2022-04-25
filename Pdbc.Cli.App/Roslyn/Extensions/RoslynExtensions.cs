using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Model;

namespace Pdbc.Cli.App.Roslyn.Extensions
{
    public static class RoslynExtensions
    {
        // Move to extension method ??
        public static CompilationUnitSyntax AddUsingStatements(this CompilationUnitSyntax @namespace, params string[] statements)
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

        public static MethodDeclarationSyntax AddMethodParameter(this MethodDeclarationSyntax syntax, 
            string parameterType,
            String parameterName)
        {
            return syntax
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                    .WithType(SyntaxFactory.ParseTypeName(parameterType)));
        }
        public static MethodDeclarationSyntax AddMethodParameters(this MethodDeclarationSyntax syntax,
            IList<PropertyItem> items)
        {
            foreach (var item in items)
            {
                syntax = AddMethodParameter(syntax, item.Type, item.Name);
            }

            return syntax;
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