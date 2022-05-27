
using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation.Api
{
    public static class ApiRequestGenerator
    {
        public static async Task GenerateApiRequestClass(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.ApiRequestClassName;
            var subfolders = new[] {"Requests", service.GenerationContext.PluralEntityName};
           
            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Api.Contracts");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);
            
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                // TODO Get the correct base class 
                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddAertssenFrameworkContractUsingStatements()
                    .AddBaseClass(service.GenerationContext.ActionInfo.ApiRequestBaseClassName)
                    .AddHttpMethodAttribute(service.GenerationContext.ActionInfo.HttpMethodAttributeUrl,
                        service.GenerationContext.ActionInfo.HttpMethodAttributeMethod)
                    .Build();
                ;
                await service.FileHelperService.WriteFile(fullFilename, entity);

            }

            if (service.GenerationContext.ActionInfo.RequiresActionDto)
            {
                entity = await service.GenerateActionDtoClassProperty(entity, fullFilename);
            }

            if (service.GenerationContext.ActionInfo.IsGetAction || service.GenerationContext.ActionInfo.IsDeleteAction)
            {
                entity = await service.GenerateIdentifierMandatoryProperty(entity, fullFilename);
            }
        }
    }
}