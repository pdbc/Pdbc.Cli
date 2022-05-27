using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Services
{
    public static class ServiceContractInterfaceGenerator
    {
        public static async Task GenerateServiceContractInterface(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.ServiceContractName.ToInterface();
            var subfolders = new[] {"Services", service.GenerationContext.PluralEntityName};

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Api.Contracts");
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
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddAertssenFrameworkContractUsingStatements()
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }


            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                    .IsInterfaceMethod(true)
                    .WithReturnType($"Task<{service.GenerationContext.ActionInfo.ApiResponseClassNameOverride}>")
                    .AddParameter(service.GenerationContext.ActionInfo.ApiRequestClassName, "request"),
                fullFilename);
        }
    }
}