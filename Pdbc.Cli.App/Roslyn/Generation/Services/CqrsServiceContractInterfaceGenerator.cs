using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Services
{
    public static class CqrsServiceContractInterfaceGenerator
    {
        public static async Task GenerateCqrsServiceContractInterface(this GenerationService service)
        {
            var className = service.GenerationContext.CqrsServiceContractName.ToInterface();
            var subfolders = new[] {"Services", service.GenerationContext.PluralEntityName};

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Services.Cqrs");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            // Generate the entity

            var entity = await roslynProjectContext.GetInterfaceByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new InterfaceDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForRequests())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForServices())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddAertssenFrameworkContractUsingStatements()

                    .AddBaseClass(service.GenerationContext.ServiceContractName.ToInterface())
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);

            }

        }
    }
}