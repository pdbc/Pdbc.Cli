using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Api
{
    public static class ApiResponseTestDataBuilderGenerator
    {
        public static async Task GenerateApiResponseClassTestDataBuilder(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.ApiResponseClassName.ToTestDataBuilder();
            var subfolders = new[] { "Api", service.GenerationContext.PluralEntityName };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Tests.Helpers");
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
                .AddUsingStatement(service.GenerationContext.GetNamespaceForServices())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForRequests())
                .AddAertssenFrameworkCoreUsingStatements()
                .AddUnitTestUsingStatement()
                .AddBaseClass($"{service.GenerationContext.ActionInfo.ApiResponseClassName.ToBuilder()}")
                .Build();

            await service.FileHelperService.WriteFile(fullFilename, entity);

        }
    }
}