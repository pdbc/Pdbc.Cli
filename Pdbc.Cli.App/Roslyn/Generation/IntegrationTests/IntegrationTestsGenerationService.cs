using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation.IntegrationTests
{
    public static class BaseServiceIntegrationTestGenerator
    {
        public static async Task GenerateBaseServiceIntegrationTest(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.ServiceContractName.ToTest();
            var subfolders = new[] {"IntegrationTests", service.GenerationContext.PluralEntityName};

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Integration.Tests");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName($"{className}<TResult>")
                    .ForNamespace(entityNamespace)
                    .IsAbstract(true)
                    .AddBaseClass(
                        $"{service.GenerationContext.BaseIntegrationTestClass}<TResult> where TResult : AertssenResponse")
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForData())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForServices())
                    .AddAertssenFrameworkContractUsingStatements()
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await service.Save(entity, new VariableDeclarationSyntaxBuilder()
                .WithName($"Service")
                .ForType($"{service.GenerationContext.ActionInfo.ServiceContractName.ToInterface()}")
                .WithModifier(SyntaxKind.ProtectedKeyword), fullFilename);


            entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter(service.GenerationContext.ActionInfo.ServiceContractName.ToInterface(), "service")
                    .AddParameter(service.GenerationContext.ApplicationName.ToDbContext(), "context")
                    .AddBaseParameter("context")
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new AssignmentSyntaxBuilder("Service", "service"))
                ,
                fullFilename);
        }
    }
}
