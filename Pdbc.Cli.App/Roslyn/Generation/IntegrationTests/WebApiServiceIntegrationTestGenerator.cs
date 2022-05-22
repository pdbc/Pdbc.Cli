using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.IntegrationTests
{
    public static class WebApiServiceIntegrationTestGenerator
    {

        public static async Task GenerateWebApiIntegrationTest(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.ActionOperationName.ToSpecification();
            var subfolders = new[] {service.GenerationContext.PluralEntityName};

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Api");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddBaseClass($"{service.GenerationContext.BaseWebApiIntegrationTestClass}")
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForData())
                    .AddUsingStatement(service.GenerationContext.GeNameForServiceAgents())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForIntegrationTests())
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
                    .AddStatement(new StatementSyntaxBuilder(
                        $"var service = new {service.GenerationContext.WebApiServiceContractName}(CreateWebApiProxy, ServiceProvider.GetService<ILogger<{service.GenerationContext.WebApiServiceContractName}>>());"))
                    .AddStatement(new StatementSyntaxBuilder(
                        $"return new {service.GenerationContext.ActionInfo.ActionOperationName.ToIntegrationTest()}(service, Context);"))
                ,
                fullFilename);
        }
    }
}