using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Entity
{
    public static class EntityTestDataBuilderGenerator
    {
        public static async Task GenerateEntityTestDataBuilder(this GenerationService service)
        {
            var className = service.GenerationContext.EntityName.ToTestDataBuilder();
            var subfolders = new[] { "Domain", service.GenerationContext.PluralEntityName };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Tests.Helpers");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

            entity = ClassDeclarationSyntaxBuilder.ForTestDataBuilder(className, entityNamespace, service.GenerationContext.EntityName)
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                .AddAertssenFrameworkCoreUsingStatements()
                .AddBaseClass($"IExternallyIdentifiableObjectBuilder<{service.GenerationContext.EntityName.ToBuilder()}>")
                .Build();

            await service.FileHelperService.WriteFile(fullFilename, entity);


        }
    }
}