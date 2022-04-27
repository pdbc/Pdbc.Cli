﻿using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Roslyn.Extensions
{
    public static class TypeDeclarationSyntaxExtensions
    {
        public static FieldDeclarationSyntax FindVariableDeclarationSyntaxFor(this TypeDeclarationSyntax typeSyntax, string name)
        {
            foreach (var property in typeSyntax.Members.OfType<FieldDeclarationSyntax>())
            {

                foreach (var v in property.Declaration.Variables)
                {
                    if (v.Identifier.ValueText == name)
                        return property;
                }
            }

            return null;
        }

        public static PropertyDeclarationSyntax FindPropertyDeclarationSyntaxFor(this TypeDeclarationSyntax typeSyntax, string name)
        {
            foreach (var property in typeSyntax.Members.OfType<PropertyDeclarationSyntax>())
            {
                if (property.Identifier.ValueText == name)
                    return property;
            }

            return null;
        }
        public static MethodDeclarationSyntax FindMethodDeclarationSyntaxFor(this TypeDeclarationSyntax typeSyntax, string name)
        {
            foreach (var method in typeSyntax.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.Identifier.ValueText == name)
                    return method;
            }

            return null;
        }
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
    }
}