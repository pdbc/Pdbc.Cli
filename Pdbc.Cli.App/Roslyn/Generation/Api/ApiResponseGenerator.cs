using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Api
{
    public static class ApiResponseGenerator
    {
        public static async Task GenerateApiResponseClass(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.RequestOutputClassName;
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

            if (service.GenerationContext.RequiresDataDto)
            {
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithName(service.GenerationContext.EntityName)
                    .ForType(service.GenerationContext.DataDtoClass), fullFilename);
            }
        }
    }
}