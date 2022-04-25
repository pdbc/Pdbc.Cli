﻿using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App
{
    public class RoslynFactory
    {
        public PropertyDeclarationSyntax GenerateProperty(String type, String name)
        {
            var propertyDeclarationExternalSystem = SyntaxFactory
                .PropertyDeclaration(SyntaxFactory.ParseTypeName(type), name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            return propertyDeclarationExternalSystem;
        }

        public ClassDeclarationSyntax CreateClassDeclaration(string name)
        {
            return SyntaxFactory.ClassDeclaration(name);
        }

        public InterfaceDeclarationSyntax CreateInterfaceDeclaration(string name)
        {
            return SyntaxFactory.InterfaceDeclaration(name);
        }


        public MethodDeclarationSyntax GeneratePublicOverridableMethod(String methodName, String returnType)
        {
            MethodDeclarationSyntax method = SyntaxFactory
                    .MethodDeclaration(SyntaxFactory.ParseTypeName(returnType), methodName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                ;

            return method;
        }
        public MethodDeclarationSyntax GenerateProtectedOverridableMethod(String methodName, String returnType)
        {
            MethodDeclarationSyntax method = SyntaxFactory
                    .MethodDeclaration(SyntaxFactory.ParseTypeName(returnType), methodName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                ;

            return method;
        }
        public MethodDeclarationSyntax GeneratePublicMethod(String methodName, String returnType)
        {
            MethodDeclarationSyntax method = SyntaxFactory
                    .MethodDeclaration(SyntaxFactory.ParseTypeName(returnType), methodName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                ;

            return method;
        }
    }
}