using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;

namespace Pdbc.Cli.App.Roslyn.Generation.Dto
{
    public static class EntityActionInterfaceDtoGenerator
    {
        public static async Task GenerateEntityActionInterfaceDto(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToInterface();
            var subfolders = new[] {service.GenerationContext.PluralEntityName};

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Dto");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetInterfaceByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new InterfaceDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddBaseClass($"IInterfacingDto")
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            if (service.GenerationContext.ActionInfo.IsStoreAction)
            {
                entity = await service.GenerateIdentifierOptionalProperty(entity, fullFilename);

            }

        }
    }
}