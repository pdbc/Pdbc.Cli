using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;

namespace Pdbc.Cli.App.Roslyn.Generation.Cqrs
{
    public static class CqrsOutputClassGenerator
    {
        public static async Task GenerateCqrsOutputClass(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.CqrsOutputClassName;
            var subfolders = new[]
            {
                "CQRS",
                service.GenerationContext.PluralEntityName,
                service.GenerationContext.ActionName
            };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Core");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);


            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);

            }

            if (service.GenerationContext.ActionInfo.RequiresDataDto)
            {
                entity = await service.GenerateDataDtoInterfaceProperty(entity, fullFilename);
                //entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder().WithName(service.GenerationContext.EntityName).ForType(service.GenerationContext.DataDtoInterface), fullFilename);
            }
        }
    }
}