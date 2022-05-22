using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.IntegrationTests
{
    public static class CqrsServiceIntegrationTestGenerator
    {

        public static async Task GenerateCqrsIntegrationTest(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.ActionOperationName.ToSpecification();
            var subfolders = new[] { "Cqrs", service.GenerationContext.PluralEntityName };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Core");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                //using Locations.Integration.Tests.IntegrationTests.Routes;
                //using Locations.Services.Cqrs.Services.Routes;
                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddBaseClass($"{service.GenerationContext.BaseCqrsIntegrationTestClass}")
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForData())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForIntegrationTests())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForCqrsServices())
                    .AddUsingStatement("Microsoft.Extensions.DependencyInjection")
                    .AddUsingStatement("Microsoft.Extensions.Logging")
                    .AddUsingAertssenFrameworkInfraTests()
                    .AddAertssenFrameworkContractUsingStatements()
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            
            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("CreateIntegrationTest")
                    .WithReturnType("IIntegrationTest")
                    .IsOverride(true)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder($"var service = ServiceProvider.GetRequiredService<{service.GenerationContext.CqrsServiceContractName.ToInterface()}>();"))
                    .AddStatement(new StatementSyntaxBuilder($"return new {service.GenerationContext.ActionInfo.ActionOperationName.ToIntegrationTest()}(service, Context);"))
                ,
                fullFilename);
        }
        
    }
}