using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Cqrs
{
    public static class CqrsInputClassTestDataBuilderGenerator
    {
        public static async Task GenerateCqrsInputClassTestDataBuilder(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.CqrsInputClassName.ToTestDataBuilder();
            var subfolders = new[] {"CQRS", service.GenerationContext.PluralEntityName};

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
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForCoreCqrs())
                .AddAertssenFrameworkCoreUsingStatements()
                .AddUnitTestUsingStatement()
                .AddBaseClass($"{service.GenerationContext.ActionInfo.CqrsInputClassName.ToBuilder()}")
                .Build();

            await service.FileHelperService.WriteFile(fullFilename, entity);

        }
    }
}