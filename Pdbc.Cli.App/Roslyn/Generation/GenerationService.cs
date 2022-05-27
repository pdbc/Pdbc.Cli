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
    public class GenerationService
    {
        public readonly RoslynSolutionContext RoslynSolutionContext;
        public readonly FileHelperService FileHelperService;
        public readonly GenerationContext GenerationContext;


        public GenerationService(RoslynSolutionContext roslynSolutionContext,
            FileHelperService fileHelperService,
            GenerationContext context)
        {
            RoslynSolutionContext = roslynSolutionContext;
            FileHelperService = fileHelperService;
            GenerationContext = context;
        }


        public async Task<TSyntaxNode> Save<TSyntaxNode>(TSyntaxNode entity,
            VariableDeclarationSyntaxBuilder property,
            String filename) where TSyntaxNode : TypeDeclarationSyntax
        {
            bool isAltered;
            var updatedEntity = entity.AddVariableIfNotExists(property, out isAltered);
            if (isAltered)
                return await SaveAndUpdate(entity, updatedEntity, filename);

            return entity;
        }
        public async Task<TSyntaxNode> Save<TSyntaxNode>(TSyntaxNode entity,
            PropertyDeclarationSyntaxBuilder property,
            String filename) where TSyntaxNode : TypeDeclarationSyntax
        {
            bool isAltered;
            var updatedEntity = entity.AddPropertyIfNotExists(property, out isAltered);
            if (isAltered)
                return await SaveAndUpdate(entity, updatedEntity, filename);

            return entity;
        }

        public async Task<TSyntaxNode> Save<TSyntaxNode>(TSyntaxNode entity,
            MethodDeclarationSyntaxBuilder method,
            String filename) where TSyntaxNode : TypeDeclarationSyntax
        {
            bool isAltered;
            var updatedEntity = entity.AddMethodToClassIfNotExists(method, out isAltered);
            if (isAltered)
                return await SaveAndUpdate(entity, updatedEntity, filename);

            return entity;
        }

        public async Task<TSyntaxNode> Save<TSyntaxNode>(TSyntaxNode entity,
            ConstructorDeclarationSyntaxBuilder constructor,
            String filename) where TSyntaxNode : TypeDeclarationSyntax
        {
            bool isAltered;
            var updatedEntity = entity.AddConstructorToClassIfNotExists(constructor, out isAltered);
            if (isAltered)
                return await SaveAndUpdate(entity, updatedEntity, filename);

            return entity;
        }


        public async Task<TSyntaxNode> AppendUsingStatement<TSyntaxNode>(TSyntaxNode entity,
            string usingStatement,
            string filename) where TSyntaxNode : TypeDeclarationSyntax
        {
            var originalNamespace = entity.GetParentNodeOfType<NamespaceDeclarationSyntax>();
            var originalCompilationSyntax = originalNamespace.GetParentNodeOfType<CompilationUnitSyntax>();

            foreach (UsingDirectiveSyntax usingDirectiveSyntax in originalCompilationSyntax.Usings)
            {
                var x = usingDirectiveSyntax.Name.ToFullString();
                if (x == usingStatement)
                    return entity;
                //NameSyntax name = usingDirectiveSyntax.Name;

                //if (name is IdentifierNameSyntax identifierNameSyntax)
                //{
                //    if (identifierNameSyntax.Identifier.ValueText == usingStatement)
                //    {
                //        return entity;
                //    }
                //}
                //else
                //{
                //    if (name is QualifiedNameSyntax qualifiedNameSyntax)
                //    {
                //        var n = qualifiedNameSyntax.ToFullString();
                //        //qualifiedNameSyntax.
                //    }
                //}
            }

            var updatedCompilationSyntax = originalCompilationSyntax.AddUsingStatements(usingStatement);

            var code = updatedCompilationSyntax.NormalizeWhitespace().ToFullString();
            await FileHelperService.WriteFile(filename, code);

            return updatedCompilationSyntax.GetSyntaxNodeFrom<TSyntaxNode>();

        }

        public async Task<TSyntax> SaveAndUpdate<TSyntax>(TSyntax original,
            TSyntax updated,
            string filename) where TSyntax : SyntaxNode
        {

            //var fullFilename = original.SyntaxTree.FilePath;

            var originalNamespace = original.GetParentNodeOfType<NamespaceDeclarationSyntax>();
            var originalCompilationSyntax = originalNamespace.GetParentNodeOfType<CompilationUnitSyntax>();


            var updatedNamespace = originalNamespace.ReplaceNode(original, updated);
            var updatedCompilationSyntax = originalCompilationSyntax.ReplaceNode(originalNamespace, updatedNamespace);

            var code = updatedCompilationSyntax.NormalizeWhitespace().ToFullString();
            await FileHelperService.WriteFile(filename, code);

            return updatedCompilationSyntax.GetSyntaxNodeFrom<TSyntax>();
        }
    }
}