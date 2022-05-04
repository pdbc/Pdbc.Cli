using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Model.Items;
using Pdbc.Cli.App.Roslyn.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Legacy
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

        public FieldDeclarationSyntax GenerateFieldVariable(String type, String name)
        {

            var propertyDeclarationExternalSystem = SyntaxFactory
                .FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(type))
                    .WithVariables(SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(name)))))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
           
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

        public ConstructorDeclarationSyntax GenerateConstructorCallingBase(String className, IList<PropertyItem> arguments)
        {
            var parameterList = ParameterList(SeparatedList<ParameterSyntax>());
            foreach (var p in arguments)
            {
                parameterList =
                    parameterList.AddParameters(Parameter(Identifier(p.Name))
                        .WithType(SyntaxFactory.ParseTypeName(p.Type)));
            }

            var argumentList = ArgumentList(SeparatedList<ArgumentSyntax>());
            foreach (var p in arguments)
            {
                argumentList =
                    argumentList.AddArguments(Argument(IdentifierName(p.Name)));
            }

            ConstructorDeclarationSyntax method = ConstructorDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithParameterList(parameterList)
                .WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                    .WithArgumentList(argumentList))
                .WithBody(Block());
                    

            return method;
        }

        public CompilationUnitSyntax CreateCompilationUnitSyntax(string[] usingStatements)
        {
            CompilationUnitSyntax compilationUnitSyntax = SyntaxFactory.CompilationUnit()
                .AddUsingStatements(usingStatements);
            ;
            return compilationUnitSyntax;
        }

        public NamespaceDeclarationSyntax CreateNamespaceDeclarationSyntax(string namespaceName)
        {
            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceName)).NormalizeWhitespace();
        }


        public AttributeSyntax CreateAttribute(string name)
        {
            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(name));
        }

        public VariableDeclarationSyntax GenerateVariableDeclaration(string entityName)
        {
            //var propertyDeclarationExternalSystem = SyntaxFactory
            //    .PropertyDeclaration(SyntaxFactory.ParseTypeName(type), name)
            //    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            //    .AddAccessorListAccessors(
            //        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            //        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            //return propertyDeclarationExternalSystem;

            var type = "var";
            var variableName = "dto";

            return SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(type))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                        SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(variableName))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("request"),
                                        SyntaxFactory.IdentifierName(entityName))))));


        }

        public VariableDeclarationSyntax GenerateVariableDeclarationLoadbyExternalSystemAndIdentifier()
        {
            //var entity = await _repository.GetByExternalIdentificationAsync(dto.ExternalSystem, dto.ExternalIdentification);
            var type = "var";
            var variableName = "entity";

            return
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(type))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                            SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier(variableName))
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.AwaitExpression(
                                            SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.IdentifierName("_repository"),
                                                        SyntaxFactory.IdentifierName("GetByExternalIdentificationAsync")))
                                                .WithArgumentList(
                                                    SyntaxFactory.ArgumentList(
                                                        SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                            new SyntaxNodeOrToken[]
                                                            {
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.IdentifierName("dto"),
                                                                        SyntaxFactory.IdentifierName("ExternalSystem"))),
                                                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.IdentifierName("dto"),
                                                                        SyntaxFactory.IdentifierName("ExternalIdentification")))
                                                            }))))))));


        }
    }
}