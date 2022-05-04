using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation
{
    public abstract class BaseGenerationService
    {
        protected readonly RoslynSolutionContext _roslynSolutionContext;
        protected readonly FileHelperService _fileHelperService;
        protected readonly GenerationContext _generationContext;


        protected BaseGenerationService(RoslynSolutionContext roslynSolutionContext,
            FileHelperService fileHelperService,
            GenerationContext context
        )
        {
            _roslynSolutionContext = roslynSolutionContext;
            _fileHelperService = fileHelperService;
            _generationContext = context;
        }
        public async Task<ClassDeclarationSyntax> Save(ClassDeclarationSyntax entity,
            VariableDeclarationSyntaxBuilder property,
            String filename)
        {
            var updatedEntity = entity.AddVariableToClassIfNotExists(property);
            return await SaveAndUpdate(entity, updatedEntity, filename);
        }
        public async Task<ClassDeclarationSyntax> Save(ClassDeclarationSyntax entity, 
            PropertyDeclarationSyntaxBuilder property,
            String filename)
        {
            var updatedEntity = entity.AddPropertyToClassIfNotExists(property);
            return await SaveAndUpdate(entity, updatedEntity, filename);
        }

        public async Task<ClassDeclarationSyntax> Save(ClassDeclarationSyntax entity,
            MethodDeclarationSyntaxBuilder method,
            String filename)
        {
            var updatedEntity = entity.AddMethodToClassIfNotExists(method);
            return await SaveAndUpdate(entity, updatedEntity, filename);
        }

        public async Task<ClassDeclarationSyntax> Save(ClassDeclarationSyntax entity,
            ConstructorDeclarationSyntaxBuilder constructor,
            String filename)
        {
            var updatedEntity = entity.AddConstructorToClassIfNotExists(constructor);
            return await SaveAndUpdate(entity, updatedEntity, filename);
        }

        protected async Task<ClassDeclarationSyntax> SaveAndUpdate(ClassDeclarationSyntax original,
            ClassDeclarationSyntax updated, 
            string filename)
        {
            var originalNamespace = original.GetParentNodeOfType<NamespaceDeclarationSyntax>();
            var originalCompilationSyntax = originalNamespace.GetParentNodeOfType<CompilationUnitSyntax>();


            var updatedNamespace = originalNamespace.ReplaceNode(original, updated);
            var updatedCompilationSyntax = originalCompilationSyntax.ReplaceNode(originalNamespace, updatedNamespace);

            var code = updatedCompilationSyntax.NormalizeWhitespace().ToFullString();
            await _fileHelperService.WriteFile(filename, code);

            return updatedCompilationSyntax.GetClassDeclarationSyntaxFrom();
        }

        //NamespaceDeclarationSyntax @namespace = classDeclarationSyntax.GetParentNodeOfType<NamespaceDeclarationSyntax>();
        //    if (@namespace != null)
        //{
        //    var updatedRoot = @namespace.ReplaceNode(classDeclarationSyntax, updatedClassDeclarationSyntax).NormalizeWhitespace();

        //    var @compilationUnitSyntax = @namespace.GetParentNodeOfType<CompilationUnitSyntax>();
        //    if (@compilationUnitSyntax == null)
        //    {
        //        throw new InvalidOperationException("Compilation Unit not found.");
        //    }

        //    var updateCompilationSyntax = @compilationUnitSyntax.ReplaceNode(@namespace, updatedRoot);
        //    var code = updateCompilationSyntax.NormalizeWhitespace().ToFullString();

        //    await _fileHelperService.WriteFile(fullFilename, code);
        //    {
        //        var result = updateCompilationSyntax.GetClassDeclarationSyntaxFrom();
        //        return result;
        //    }
        //}

    }
}