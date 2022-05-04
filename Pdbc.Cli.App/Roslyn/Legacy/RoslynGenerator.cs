using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Model.Items;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Legacy
{
    public class RoslynGenerator
    {
        private readonly FileHelperService _fileHelperService;
        private readonly RoslynFactory _roslynFactory;

        public RoslynGenerator(FileHelperService fileHelperService)
        {
            _fileHelperService = fileHelperService;
            _roslynFactory = new RoslynFactory();
        }

        public async Task<ClassDeclarationSyntax> GeneratePublicClass(string fullFilename,
            string entityNamespace,
            string className,
            string[] usingStatements,
            string[] baseClasses)
        {
            var compilationUnitSyntax = _roslynFactory.CreateCompilationUnitSyntax(usingStatements);

            var @namespace = _roslynFactory.CreateNamespaceDeclarationSyntax(entityNamespace);

            //  Create a class
            var classDeclaration = _roslynFactory.CreateClassDeclaration(className)
                .AsPublic()
                .AddBaseClasses(baseClasses);

            // Add class to namespace
            @namespace = @namespace.AddMembers(classDeclaration);

            compilationUnitSyntax = compilationUnitSyntax.AddMembers(@namespace);

            // Generate the code
            var code = compilationUnitSyntax
                .NormalizeWhitespace()
                .ToFullString();

            // write the file to disk
            await _fileHelperService.WriteFile(fullFilename, code);

            var result = compilationUnitSyntax.GetClassDeclarationSyntaxFrom();
            return result;
        }

        public async Task<InterfaceDeclarationSyntax> GeneratePublicInterface(
            string fullFilename,
            string entityNamespace,
            string className,
            string[] usingStatements,
            string[] baseClasses)
        {
            var compilationUnitSyntax = _roslynFactory.CreateCompilationUnitSyntax(usingStatements);

            var @namespace = _roslynFactory.CreateNamespaceDeclarationSyntax(entityNamespace);

            //  Create a class
            var interfaceDeclarationSyntax = _roslynFactory.CreateInterfaceDeclaration(className)
                .AsPublic()
                .AddBaseClasses(baseClasses);


            // Add class to namespace
            @namespace = @namespace.AddMembers(interfaceDeclarationSyntax);

            compilationUnitSyntax = compilationUnitSyntax.AddMembers(@namespace);

            // Generate the code
            var code = compilationUnitSyntax
                .NormalizeWhitespace()
                .ToFullString();

            // write the file to disk
            await _fileHelperService.WriteFile(fullFilename, code);
            var result = compilationUnitSyntax.GetInterfaceDeclarationSyntaxFrom();
            return result;
        }

        public async Task<ClassDeclarationSyntax> GenerateTestSpecificationClass(
            string fullFilename,
            string entityNamespace,
            string className,
            string[] usingStatements,
            string[] baseClasses)
        {
            var compilationUnitSyntax = _roslynFactory.CreateCompilationUnitSyntax(usingStatements);

            var @namespace = _roslynFactory.CreateNamespaceDeclarationSyntax(entityNamespace);


            //  Create a class
            var classDeclaration = _roslynFactory.CreateClassDeclaration(className)
                .AsPublic()
                .AddBaseClasses(baseClasses)
                .AddAttribute("TestFixture"); ;


            // Add class to namespace
            @namespace = @namespace.AddMembers(classDeclaration);

            compilationUnitSyntax = compilationUnitSyntax.AddMembers(@namespace);

            // Generate the code
            var code = compilationUnitSyntax
                .NormalizeWhitespace()
                .ToFullString();

            // write the file to disk
            await _fileHelperService.WriteFile(fullFilename, code);
            var result = compilationUnitSyntax.GetClassDeclarationSyntaxFrom();
            return result;
        }


        public async Task<ClassDeclarationSyntax> AppendProperty(string fullFilename,
            ClassDeclarationSyntax classDeclarationSyntax,
            PropertyItem property)
        {
            var propertyDeclarationSyntax = classDeclarationSyntax.FindPropertyDeclarationSyntaxFor(property.Name);
            if (propertyDeclarationSyntax != null)
                return classDeclarationSyntax;

            //var oldCode = classDeclarationSyntax.ToFullString();
            var propertyDeclarationExternalSystem = _roslynFactory.GenerateProperty(property.Type, property.Name);
            var updatedClassDeclarationSyntax = classDeclarationSyntax.AddAndKeep(propertyDeclarationExternalSystem);

            return await SaveClassDeclaration(fullFilename, classDeclarationSyntax, updatedClassDeclarationSyntax) as ClassDeclarationSyntax;

        }

        public async Task<ClassDeclarationSyntax> AppendClassVariable(string fullFilename,
            ClassDeclarationSyntax classDeclarationSyntax,
            PropertyItem property)
        {
            var propertyDeclarationSyntax = classDeclarationSyntax.FindVariableDeclarationSyntaxFor(property.Name);
            if (propertyDeclarationSyntax != null)
                return classDeclarationSyntax;

            //var oldCode = classDeclarationSyntax.ToFullString();
            var propertyDeclarationExternalSystem = _roslynFactory.GenerateFieldVariable(property.Type, property.Name);
            var updatedClassDeclarationSyntax = classDeclarationSyntax.AddAndKeep(propertyDeclarationExternalSystem);

            return await SaveClassDeclaration(fullFilename, classDeclarationSyntax, updatedClassDeclarationSyntax) as ClassDeclarationSyntax;

        }

        public async Task<ClassDeclarationSyntax> AppendProtectedOverridableMethod(string fullFilename,
                ClassDeclarationSyntax classDeclarationSyntax,
                MethodItem method, List<PropertyItem> properties,
                string body)
        {
            var methodDeclarationSyntax = classDeclarationSyntax.FindMethodDeclarationSyntaxFor(method.Name);
            if (methodDeclarationSyntax != null)
                return classDeclarationSyntax;

            methodDeclarationSyntax = _roslynFactory.GenerateProtectedOverridableMethod(method.Name, method.ReturnType)
                .AddMethodParameters(properties)
                .AddMethodBody(body);

            var updatedClassDeclarationSyntax = classDeclarationSyntax.AddAndKeep(methodDeclarationSyntax);

            return await SaveClassDeclaration(fullFilename, classDeclarationSyntax, updatedClassDeclarationSyntax) as ClassDeclarationSyntax;

        }


        public async Task<ClassDeclarationSyntax> AppendPublicOverridableMethod(string fullFilename,
            ClassDeclarationSyntax classDeclarationSyntax,
            MethodItem method, List<PropertyItem> properties,
            string body,
            params string[] bodyStatements)
        {
            var methodDeclarationSyntax = classDeclarationSyntax.FindMethodDeclarationSyntaxFor(method.Name);
            if (methodDeclarationSyntax != null)
                return classDeclarationSyntax;

            methodDeclarationSyntax = _roslynFactory.GeneratePublicOverridableMethod(method.Name, method.ReturnType)
                .AddMethodParameters(properties)
                .AddMethodBody(body);

            if (bodyStatements != null)
            {
                foreach (var b in bodyStatements)
                {
                    methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(SyntaxFactory.ParseStatement(b));
                }
            }


            var updatedClassDeclarationSyntax = classDeclarationSyntax.AddAndKeep(methodDeclarationSyntax);

            return await SaveClassDeclaration(fullFilename, classDeclarationSyntax, updatedClassDeclarationSyntax) as ClassDeclarationSyntax;

        }

        public async Task<ClassDeclarationSyntax> AppendPublicMethodNotImplemented(string fullFilename,
            ClassDeclarationSyntax classDeclarationSyntax,
            MethodItem method, List<PropertyItem> properties)
        {
            var methodDeclarationSyntax = classDeclarationSyntax.FindMethodDeclarationSyntaxFor(method.Name);
            if (methodDeclarationSyntax != null)
                return classDeclarationSyntax;

            methodDeclarationSyntax = _roslynFactory.GeneratePublicMethod(method.Name, method.ReturnType)
                .AddMethodParameters(properties)
                .AddMethodBody("throw new NotImplementedException();");
            
            var updatedClassDeclarationSyntax = classDeclarationSyntax.AddAndKeep(methodDeclarationSyntax);

            return await SaveClassDeclaration(fullFilename, classDeclarationSyntax, updatedClassDeclarationSyntax) as ClassDeclarationSyntax;

        }

        public async Task<ClassDeclarationSyntax> GenerateConstructor(string fullFilename,
            string className,
            ClassDeclarationSyntax classDeclarationSyntax,
            List<PropertyItem> parameters)
        {
            var constructorDeclarationSyntax = classDeclarationSyntax.FindConstructorDeclarationSyntax();
            if (constructorDeclarationSyntax != null)
                return classDeclarationSyntax;

            constructorDeclarationSyntax = _roslynFactory.GenerateConstructorCallingBase(className, parameters);

            var updatedClassDeclarationSyntax = classDeclarationSyntax.AddAndKeep(constructorDeclarationSyntax);

            return await SaveClassDeclaration(fullFilename, classDeclarationSyntax, updatedClassDeclarationSyntax) as ClassDeclarationSyntax;
            
        }


        public async Task<ClassDeclarationSyntax> AppendAssertionFailTestMethod(string fullFilename,
            ClassDeclarationSyntax classDeclarationSyntax,
            MethodItem method)
        {
            var methodDeclarationSyntax = classDeclarationSyntax.FindMethodDeclarationSyntaxFor(method.Name);
            if (methodDeclarationSyntax != null)
                return classDeclarationSyntax;

            methodDeclarationSyntax = _roslynFactory.GeneratePublicMethod(method.Name, method.ReturnType)
                .AddMethodBody(@"Assert.Fail();")
                .AddAttribute("Test"); ;

            var updatedClassDeclarationSyntax = classDeclarationSyntax.AddAndKeep(methodDeclarationSyntax);

            return await SaveClassDeclaration(fullFilename, classDeclarationSyntax, updatedClassDeclarationSyntax) as ClassDeclarationSyntax;

        }



        private async Task<TypeDeclarationSyntax> SaveClassDeclaration(string fullFilename,
            TypeDeclarationSyntax classDeclarationSyntax,
            TypeDeclarationSyntax updatedClassDeclarationSyntax)
        {
            NamespaceDeclarationSyntax @namespace = classDeclarationSyntax.GetParentNodeOfType<NamespaceDeclarationSyntax>();
            if (@namespace != null)
            {
                var updatedRoot = @namespace.ReplaceNode(classDeclarationSyntax, updatedClassDeclarationSyntax).NormalizeWhitespace();

                var @compilationUnitSyntax = @namespace.GetParentNodeOfType<CompilationUnitSyntax>();
                if (@compilationUnitSyntax == null)
                {
                    throw new InvalidOperationException("Compilation Unit not found.");
                }

                var updateCompilationSyntax = @compilationUnitSyntax.ReplaceNode(@namespace, updatedRoot);
                var code = updateCompilationSyntax.NormalizeWhitespace().ToFullString();

                await _fileHelperService.WriteFile(fullFilename, code);
                {
                    var result = updateCompilationSyntax.GetClassDeclarationSyntaxFrom();
                    return result;
                }
            }

            throw new InvalidOperationException("SaveClassDeclaration failed because namespace not found.");
        }



    //    // Complete methods
    //    public async Task<ClassDeclarationSyntax> AppendCqrsHandlerStoreMethod(string fullFilename,
    //ClassDeclarationSyntax classDeclarationSyntax,
    //StandardAction generationContext)
    //    {
    //        var properties = new List<PropertyItem>()
    //        {
    //            new PropertyItem(generationContext.ToCqrsInputClassName(), "request"),
    //            PropertyItem.CancellationTokenPropertyItem
    //        };
    //        var method = new MethodItem($"Task<{generationContext.ToCqrsOutputClassName()}>", "Handle");

    //        var methodDeclarationSyntax = classDeclarationSyntax.FindMethodDeclarationSyntaxFor(method.Name);
    //        if (methodDeclarationSyntax != null)
    //            return classDeclarationSyntax;

    //        methodDeclarationSyntax = _roslynFactory.GeneratePublicMethod(method.Name, method.ReturnType)
    //            .AddMethodParameters(properties)
    //            .WithBody(SyntaxFactory.Block(

    //                SyntaxFactory.LocalDeclarationStatement(_roslynFactory.GenerateVariableDeclaration(generationContext._entityName)),
    //                SyntaxFactory.LocalDeclarationStatement(_roslynFactory.GenerateVariableDeclarationLoadbyExternalSystemAndIdentifier()),

    //                SyntaxFactory.IfStatement(
    //                        SyntaxFactory.BinaryExpression(
    //                            SyntaxKind.EqualsExpression,
    //                            SyntaxFactory.IdentifierName("entity"),
    //                            SyntaxFactory.LiteralExpression(
    //                                SyntaxKind.NullLiteralExpression)),
    //                        SyntaxFactory.Block(
    //                            SyntaxFactory.ExpressionStatement(

    //                                SyntaxFactory.AssignmentExpression(
    //                                    SyntaxKind.SimpleAssignmentExpression,
    //                                    SyntaxFactory.IdentifierName("entity"),
    //                                    SyntaxFactory.InvocationExpression(
    //                                            SyntaxFactory.MemberAccessExpression(
    //                                                SyntaxKind.SimpleMemberAccessExpression,
    //                                                SyntaxFactory.IdentifierName("_factory"),
    //                                                SyntaxFactory.IdentifierName("Create")))
    //                                        .WithArgumentList(
    //                                            SyntaxFactory.ArgumentList(
    //                                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
    //                                                    SyntaxFactory.Argument(
    //                                                        SyntaxFactory.MemberAccessExpression(
    //                                                            SyntaxKind.SimpleMemberAccessExpression,
    //                                                            SyntaxFactory.IdentifierName("request"),
    //                                                            SyntaxFactory.IdentifierName("Project")))))))

    //                                ),
    //                            SyntaxFactory.ExpressionStatement(
    //                                SyntaxFactory.InvocationExpression(
    //                                        SyntaxFactory.MemberAccessExpression(
    //                                            SyntaxKind.SimpleMemberAccessExpression,
    //                                            SyntaxFactory.IdentifierName("_repository"),
    //                                            SyntaxFactory.IdentifierName("Insert")))
    //                                    .WithArgumentList(
    //                                        SyntaxFactory.ArgumentList(
    //                                            SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
    //                                                SyntaxFactory.Argument(
    //                                                    SyntaxFactory.IdentifierName("entity"))))))))
    //                    .WithElse(
    //                        SyntaxFactory.ElseClause(
    //                            SyntaxFactory.Block(
    //                                SyntaxFactory.ExpressionStatement(
    //                                    SyntaxFactory.InvocationExpression(
    //                                            SyntaxFactory.MemberAccessExpression(
    //                                                SyntaxKind.SimpleMemberAccessExpression,
    //                                                SyntaxFactory.IdentifierName("_changesHandler"),
    //                                                SyntaxFactory.IdentifierName("ApplyChanges")))
    //                                        .WithArgumentList(
    //                                            SyntaxFactory.ArgumentList(
    //                                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
    //                                                    new SyntaxNodeOrToken[]
    //                                                    {
    //                                                        SyntaxFactory.Argument(
    //                                                            SyntaxFactory.IdentifierName("entity")),
    //                                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
    //                                                        SyntaxFactory.Argument(
    //                                                            SyntaxFactory.MemberAccessExpression(
    //                                                                SyntaxKind.SimpleMemberAccessExpression,
    //                                                                SyntaxFactory.IdentifierName("request"),
    //                                                                SyntaxFactory.IdentifierName("Project")))
    //                                                    })))),
    //                                SyntaxFactory.ExpressionStatement(
    //                                    SyntaxFactory.InvocationExpression(
    //                                            SyntaxFactory.MemberAccessExpression(
    //                                                SyntaxKind.SimpleMemberAccessExpression,
    //                                                SyntaxFactory.IdentifierName("_repository"),
    //                                                SyntaxFactory.IdentifierName("Update")))
    //                                        .WithArgumentList(
    //                                            SyntaxFactory.ArgumentList(
    //                                                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
    //                                                    SyntaxFactory.Argument(
    //                                                        SyntaxFactory.IdentifierName("entity"))))))))),
    //                SyntaxFactory.ReturnStatement(
    //                    SyntaxFactory.InvocationExpression(
    //                        SyntaxFactory.MemberAccessExpression(
    //                            SyntaxKind.SimpleMemberAccessExpression,
    //                            SyntaxFactory.IdentifierName("Nothing"),
    //                            SyntaxFactory.IdentifierName("AtAll"))))));


    //        var updatedClassDeclarationSyntax = classDeclarationSyntax.AddAndKeep(methodDeclarationSyntax);

    //        return await SaveClassDeclaration(fullFilename, classDeclarationSyntax, updatedClassDeclarationSyntax);

    //    }


        public async Task<InterfaceDeclarationSyntax> AppendServiceContractMethod(string fullFilename,
            InterfaceDeclarationSyntax syntax, GenerationContext generationContext)
        {
            // Sample
            // Task<ListAssetsResponse> ListAssets(ListAssetsRequest request);
            var methodDeclarationSyntax = syntax.FindMethodDeclarationSyntaxFor(generationContext.ServiceActionName);
            if (methodDeclarationSyntax != null)
                return syntax;

            var property = new PropertyItem(generationContext.RequestInputClassName, "request");
            methodDeclarationSyntax = _roslynFactory.GeneratePublicMethod(generationContext.ServiceActionName, $"Task<{generationContext.RequestOutputClassName}>")
                .AddMethodParameters(new List<PropertyItem>() { property });

            var updatedClassDeclarationSyntax = syntax.AddAndKeep(methodDeclarationSyntax);

            return await SaveClassDeclaration(fullFilename, syntax, updatedClassDeclarationSyntax) as InterfaceDeclarationSyntax;
        }
    }
}