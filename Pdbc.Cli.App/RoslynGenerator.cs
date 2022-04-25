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
        
        public async Task<ClassDeclarationSyntax> GeneratePublicInterface(
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
            var result = compilationUnitSyntax.GetClassDeclarationSyntaxFrom();
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



    }
}