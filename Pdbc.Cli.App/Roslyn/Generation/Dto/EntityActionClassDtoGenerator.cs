using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;

namespace Pdbc.Cli.App.Roslyn.Generation.Dto
{
    public static class EntityActionClassDtoGenerator
    {

        public static async Task GenerateEntityActionClassDto(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.EntityActionName.ToDto();
            var subfolders = new[] { service.GenerationContext.PluralEntityName };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Dto");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddBaseClass(service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToInterface())
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await service.GenerateExternalSystemProperty(entity, fullFilename);
            entity = await service.GenerateExternalIdentificationProperty(entity, fullFilename);
            entity = await service.GenerateDateModifiedProperty(entity, fullFilename);
            
            if (service.GenerationContext.ActionInfo.IsStoreAction)
            {
                entity = await service.GenerateIdentifierOptionalProperty(entity, fullFilename);
            }
        }
    }
}