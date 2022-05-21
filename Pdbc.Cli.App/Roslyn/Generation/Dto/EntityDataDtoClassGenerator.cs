using System.Threading.Tasks;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Dto
{
    public static class EntityDataDtoClassGenerator
    {

        public static async Task GenerateEntityDataClassDto(this GenerationService service)
        {
            var className = service.GenerationContext.DataDtoClass;
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
                    .AddBaseClass(service.GenerationContext.DataDtoInterface)
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

        }
    }
}