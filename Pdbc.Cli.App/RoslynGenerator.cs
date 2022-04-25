using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Model;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Visitors;

namespace Pdbc.Cli.App
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

        public async Task<NamespaceDeclarationSyntax> GenerateNamespace(string nameSpaceString, string[] usingStatements)
        {
            var @namespace = CreateNamespaceDeclaration(nameSpaceString)
                .AddUsingStatements(usingStatements);

            return @namespace;
        }
        public async Task<ClassDeclarationSyntax> GeneratePublicClass(
            string fullFilename,
            NamespaceDeclarationSyntax @namespace,
            string className,
            string[] baseClasses)
        {
            //  Create a class
            var classDeclaration = _roslynFactory.CreateClassDeclaration(className)
                .AsPublic()
                .AddBaseClasses(baseClasses);


            // Add class to namespace
            @namespace = @namespace.AddMembers(classDeclaration);

            // Generate the code
            var code = @namespace
                .NormalizeWhitespace()
                .ToFullString();

            // write the file to disk
            await _fileHelperService.WriteFile(fullFilename, code);

            return @namespace.Members.OfType<ClassDeclarationSyntax>().FirstOrDefault();
        }

        public async Task<ClassDeclarationSyntax> GeneratePublicInterface(
            string fullFilename,
            NamespaceDeclarationSyntax @namespace,
            string className,
            string[] baseClasses)
        {
            //  Create a class
            var classDeclaration = _roslynFactory.CreateInterfaceDeclaration(className)
                .AsPublic()
                .AddBaseClasses(baseClasses);


            // Add class to namespace
            @namespace = @namespace.AddMembers(classDeclaration);

            // Generate the code
            var code = @namespace
                .NormalizeWhitespace()
                .ToFullString();

            // write the file to disk
            await _fileHelperService.WriteFile(fullFilename, code);

            return @namespace.Members.OfType<ClassDeclarationSyntax>().FirstOrDefault();
        }

        public async Task<ClassDeclarationSyntax> GenerateTestSpecificationClass(
            string fullFilename,
            NamespaceDeclarationSyntax @namespace,
            string className,
            string[] baseClasses)
        {
            //  Create a class
            var classDeclaration = _roslynFactory.CreateClassDeclaration(className)
                .AsPublic()
                .AddBaseClasses(baseClasses)
                .AddAttribute("TestFixture"); ;


            // Add class to namespace
            @namespace = @namespace.AddMembers(classDeclaration);

            // Generate the code
            var code = @namespace
                .NormalizeWhitespace()
                .ToFullString();

            // write the file to disk
            await _fileHelperService.WriteFile(fullFilename, code);

            return @namespace.Members.OfType<ClassDeclarationSyntax>().FirstOrDefault();
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
            //var updatedClassDeclarationSyntax = classDeclarationSyntax.WithMembers(classDeclarationSyntax.Members.Add(propertyDeclarationExternalSystem));
            var updatedClassDeclarationSyntax = classDeclarationSyntax.AddAndKeep(propertyDeclarationExternalSystem);

            //var root = classDeclarationSyntax.SyntaxTree.GetRoot();
            //var filePath = classDeclarationSyntax.SyntaxTree.FilePath;

            return await SaveClassDeclaration(fullFilename, classDeclarationSyntax, updatedClassDeclarationSyntax);

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

            return await SaveClassDeclaration(fullFilename, classDeclarationSyntax, updatedClassDeclarationSyntax);

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
                foreach(var b in bodyStatements) 
                {
                    methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(SyntaxFactory.ParseStatement(b));
                }
            }
            

            var updatedClassDeclarationSyntax = classDeclarationSyntax.AddAndKeep(methodDeclarationSyntax);

            return await SaveClassDeclaration(fullFilename, classDeclarationSyntax, updatedClassDeclarationSyntax);

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

            return await SaveClassDeclaration(fullFilename, classDeclarationSyntax, updatedClassDeclarationSyntax);

        }

        private async Task<ClassDeclarationSyntax> SaveClassDeclaration(string fullFilename,
            ClassDeclarationSyntax classDeclarationSyntax,
            ClassDeclarationSyntax updatedClassDeclarationSyntax)
        {
            NamespaceDeclarationSyntax @namespace = classDeclarationSyntax.GetParentNodeOfType<NamespaceDeclarationSyntax>();
            if (@namespace != null)
            {
                //var usingDirectives = @namespace.GetUsingStatementsFromNamespace<UsingDirectiveSyntax>();

                var updatedRoot = @namespace.ReplaceNode(classDeclarationSyntax, updatedClassDeclarationSyntax).NormalizeWhitespace();

                CompilationUnitSyntax @compilationUnitSyntax = @namespace.GetParentNodeOfType<CompilationUnitSyntax>();
                if (@compilationUnitSyntax == null)
                {

                    throw new InvalidOperationException("Compilation Unit not found.");
                }

                var updateCompilationSyntax = @compilationUnitSyntax.ReplaceNode(@namespace, updatedRoot);
                var code = updateCompilationSyntax.NormalizeWhitespace().ToFullString();
                //var code = @namespace.NormalizeWhitespace().ToFullString();

                await _fileHelperService.WriteFile(fullFilename, code);

                {
                    return updatedRoot.Members.OfType<ClassDeclarationSyntax>().FirstOrDefault();
                }
            }

            throw new InvalidOperationException("SaveClassDeclaration failed because namespace not found.");
        }


        public NamespaceDeclarationSyntax CreateNamespaceDeclaration(string @namespace)
        {
            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@namespace)).NormalizeWhitespace();
        }



        public InterfaceDeclarationSyntax CreateInterfaceDeclaration(string name)
        {
            return SyntaxFactory.InterfaceDeclaration(name);
        }



        public AttributeSyntax CreateAttribute(string name)
        {
            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(name));
        }
    }
}