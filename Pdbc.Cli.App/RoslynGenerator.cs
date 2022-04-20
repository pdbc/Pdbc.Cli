using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Visitors;

namespace Pdbc.Cli.App
{
    public class RoslynGenerator
    {
        public async Task<List<ClassDeclarationSyntax>> GetclassesFromProject(Project project)
        {
            var compilation = await project.GetCompilationAsync();
            
            var classVisitor = new ClassVirtualizationVisitor();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                classVisitor.Visit(syntaxTree.GetRoot());
            }


            //var modelFilePath = Path.Combine(Path.GetDirectoryName(project.FilePath), "model");
            var classes = classVisitor.Classes;

            return classes;
        }

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

        public NamespaceDeclarationSyntax CreateNamespaceDeclaration(string @namespace)
        {
            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@namespace)).NormalizeWhitespace();
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

        public MethodDeclarationSyntax GeneratePublicMethod(String methodName, String returnType)
        {
            MethodDeclarationSyntax method = SyntaxFactory
                    .MethodDeclaration(SyntaxFactory.ParseTypeName(returnType), methodName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                ;

            return method;
        }

        public AttributeSyntax CreateAttribute(string name)
        {
            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(name));
        }
    }
}