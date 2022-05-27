using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;

namespace Pdbc.Cli.App.Roslyn.Generation.Api
{
    public static class ApiResponseGenerator
    {
        public static async Task GenerateApiResponseClass(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.ApiResponseClassName;
            var subfolders = new[] {"Requests", service.GenerationContext.PluralEntityName};
            ;


            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Api.Contracts");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddAertssenFrameworkContractUsingStatements()
                    .AddBaseClass(service.GenerationContext.ActionInfo.ApiResponseBaseClassName)
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            if (service.GenerationContext.ActionInfo.RequiresDataDto && 
                !service.GenerationContext.ActionInfo.IsListAction)
            {
                entity = await service.GenerateDataDtoClassProperty(entity, fullFilename);
            }
        }
    }
}