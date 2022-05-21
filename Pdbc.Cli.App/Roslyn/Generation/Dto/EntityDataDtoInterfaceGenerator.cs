using System.Threading.Tasks;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Dto
{
    public static class EntityDataDtoInterfaceGenerator
    {
        public static async Task GenerateEntityDataInterfaceDto(this GenerationService service)
        {
            var className = service.GenerationContext.DataDtoInterface;
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
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }
        }
    }
}