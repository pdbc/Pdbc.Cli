using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Cqrs.UnitTests
{
    public static class GenerateCqrsHandlerUnitTestClassForStoreUpdateGenerator
    {

        public static async Task GenerateCqrsHandlerUnitTestClassForStoreUpdate(this GenerationService service)
        {
            var className = $"Update{service.GenerationContext.CqrsHandlerClassName}".ToSpecification();
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
                .AddBaseClass($"{service.GenerationContext.CqrsHandlerClassName.ToSpecification()}")
                .Build();

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("CreateCommand")
                    .IsOverride(true)
                    .WithReturnType(service.GenerationContext.ActionInfo.CqrsInputClassName)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement(
                        $"return new {service.GenerationContext.ActionInfo.CqrsInputClassName.ToTestDataBuilder()}();"))
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("GetCreatedItem")
                    .IsOverride(true)
                    .WithReturnType(service.GenerationContext.EntityName)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement($"return null;"))
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("GetLoadedItem")
                    .IsOverride(true)
                    .WithReturnType(service.GenerationContext.EntityName)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(
                        new StatementSyntaxBuilder().AddStatement(
                            $"return new {service.GenerationContext.EntityName.ToTestDataBuilder()}();"))
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_repository_called_to_load_the_entity")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement(
                        $"Repository.AssertWasCalled(x => x.GetById(Command.{service.GenerationContext.EntityName}.Id.Value));;"))
                ,
                fullFilename);


            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_changes_handler_called_to_update_the_object")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement(
                        $"ChangesHandler.AssertWasCalled(x => x.ApplyChanges(LoadedItem, Command.{service.GenerationContext.EntityName}));"))
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_repository_called_to_update_the_object")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(
                        new StatementSyntaxBuilder().AddStatement(
                            $"Repository.AssertWasCalled(x => x.Update(LoadedItem));"))
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_dbcontextservice_called_to_savechanges")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement(
                        $"{service.GenerationContext.ApplicationName}DbContextService.AssertWasCalled(x => x.SaveChangesAsync(CancellationToken));"))
                ,
                fullFilename);

            await service.FileHelperService.WriteFile(fullFilename, entity);
        }
    }
}