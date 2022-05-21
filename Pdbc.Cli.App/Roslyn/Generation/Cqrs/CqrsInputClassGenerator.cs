using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;

namespace Pdbc.Cli.App.Roslyn.Generation.Cqrs
{
    public static class CqrsInputClassGenerator
    {
        public static async Task GenerateCqrsInputClass(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.CqrsInputClassName;
            var subfolders = new[]
            {
                "CQRS",
                service.GenerationContext.PluralEntityName,
                service.GenerationContext.ActionName
            };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Core");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddBaseClass(
                        $"I{service.GenerationContext.ActionInfo.CqrsInputType}<{service.GenerationContext.GetCqrsOutputClassNameBasedOnAction()}>")
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            if (service.GenerationContext.ActionInfo.RequiresActionDto)
            {
                entity = await service.GenerateActionDtoInterfaceProperty(entity, fullFilename);
            }

            if (service.GenerationContext.ActionInfo.IsGetAction || service.GenerationContext.ActionInfo.IsDeleteAction)
            {

                entity = await service.GenerateIdentifierMandatoryProperty(entity, fullFilename);
            }
        }
    }
}