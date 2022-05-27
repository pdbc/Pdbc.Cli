using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Cqrs.UnitTests
{
    public static class GenerateCqrsHandlerUnitTestClassForStoreCreateGenerator
    {
        public static async Task GenerateCqrsHandlerUnitTestClassForStoreCreate(this GenerationService service)
        {
            var className = $"Create{service.GenerationContext.ActionInfo.CqrsInputClassName.ToHandler()}".ToSpecification();
            var subfolders = new[] { "Core", "CQRS", service.GenerationContext.PluralEntityName, service.GenerationContext.ActionName };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
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
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddUsingAertssenFrameworkValidationInfra()
                .AddUsingStatement("System.Collections.Generic")
                .AddUsingStatement("Moq")
                .AddUnitTestUsingStatement()
                .AddUsingAertssenFrameworkAuditModel()
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForCoreCqrs())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForData())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDataRepositories())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForCoreCqrsTestDataBuilders())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainTestDataBuilders())
                .AddUsingStatement("AutoMapper")
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddUsingAertssenFrameworkServices()
                .AddTestFixtureAttribute(true)
                .AddBaseClass($"{service.GenerationContext.ActionInfo.CqrsInputClassName.ToHandler().ToSpecification()}")
                .Build();

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("GetCreatedItem")
                    .IsOverride(true)
                    .WithReturnType(service.GenerationContext.EntityName)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(
                        new StatementSyntaxBuilder(
                            $"return new {service.GenerationContext.EntityName.ToTestDataBuilder()}();"))
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("GetLoadedItem")
                    .IsOverride(true)
                    .WithReturnType(service.GenerationContext.EntityName)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder($"return null;"))
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_factory_called_to_setup_item")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(new StatementSyntaxBuilder(
                        $"Factory.AssertWasCalled(x => x.Create(Command.{service.GenerationContext.EntityName}));"))
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_repository_called_to_insert_the_object")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(
                        new StatementSyntaxBuilder(
                            $"Repository.AssertWasCalled(x => x.Insert(CreatedItem));"))
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_dbcontextservice_called_to_savechanges")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(new StatementSyntaxBuilder(
                        $"{service.GenerationContext.ApplicationName}DbContextService.AssertWasCalled(x => x.SaveChangesAsync(CancellationToken));"))
                ,
                fullFilename);

            await service.FileHelperService.WriteFile(fullFilename, entity);
        }
    }
}