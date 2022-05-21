using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Entity
{
    public static class EntityRepositorDataFixturesGenerator
    {
        public static async Task GenerateRepositoryBaseIntegrationTests(this GenerationService service)
        {
            var className = service.GenerationContext.EntityName.ToRepository()
                .ToSpecification();
            var subfolders = new[] { "Repositories" };


            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Data");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

            entity = new ClassDeclarationSyntaxBuilder()
                .WithName(className)
                .ForNamespace(entityNamespace)
                .AddUsingAertssenFrameworkRepositories()
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModelHelpers())
                .AddUsingAertssenFrameworkAuditModel()
                .AddUnitTestUsingStatement()
                .AddBaseClass($"BaseRepositorySpecification<{service.GenerationContext.EntityName}>")
                .Build();

            await service.FileHelperService.WriteFile(fullFilename, entity);


            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder().WithName("CreateExistingItem").WithReturnType(service.GenerationContext.EntityName)
                    .IsOverride(true)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder()
                        .ThatReturnsAndObject($"{service.GenerationContext.EntityName}TestDataBuilder"))
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder().WithName("CreateNewItem").WithReturnType(service.GenerationContext.EntityName)
                    .IsOverride(true)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder()
                        .ThatReturnsAndObject($"{service.GenerationContext.EntityName}TestDataBuilder"))
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder().WithName("EditItem")
                    .IsOverride(true)
                    .AddParameter(service.GenerationContext.EntityName, "item")
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .ThrowsNewNotImplementedException()
                ,
                fullFilename);
            
        }

        public static async Task GenerateRepositoryQueriesIntegrationTests(this GenerationService service)
        {
            var className = service.GenerationContext.EntityName.ToRepository().ToSpecification("Queries");
            var subfolders = new[] { "Extensions" };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Data");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

            entity = new ClassDeclarationSyntaxBuilder()
                .WithName(className)
                .ForNamespace(entityNamespace)

                .AddUsingAertssenFrameworkRepositories()
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDataRepositories())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForIntegationTestDataExtensions())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModelHelpers())
                .AddUnitTestUsingStatement()
                .AddUsingAertssenFrameworkAuditModel()
                .AddBaseClass($"BaseRepositoryExtensionsSpecification<{service.GenerationContext.EntityName.ToRepositoryInterface()}>")
                .Build();

            await service.FileHelperService.WriteFile(fullFilename, entity);

            entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.AssertionFailedTestMethod("Verify_extension_queries"), fullFilename);

        }
    }
}