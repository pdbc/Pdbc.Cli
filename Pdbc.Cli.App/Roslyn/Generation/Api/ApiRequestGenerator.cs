using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation.Api
{
    public static class ApiRequestGenerator
    {
        public static async Task GenerateApiRequestClass(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.RequestInputClassName;
            var subfolders = new[] {"Requests", service.GenerationContext.PluralEntityName};
            ;

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Api.Contracts");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            // Generate the entity

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
                    .AddHttpMethodAttribute(service.GenerationContext.GetHttpMethodAttributeUrlValue(),
                        service.GenerationContext.GetHttpMethodAttributeMethodValue())
                    .Build();
                ;
                await service.FileHelperService.WriteFile(fullFilename, entity);

            }

            if (service.GenerationContext.ActionInfo.RequiresActionDto)
            {
                entity = await service.Save(entity,
                    new PropertyDeclarationSyntaxBuilder().WithName(service.GenerationContext.EntityName)
                        .ForType(service.GenerationContext.ActionDtoClass), fullFilename);
            }

            if (service.GenerationContext.ActionInfo.IsGetAction || service.GenerationContext.ActionInfo.IsDeleteAction)
            {
                entity = await service.Save(entity,
                    new PropertyDeclarationSyntaxBuilder().WithName("Id").ForType("long"), fullFilename);
            }
        }
    }
}