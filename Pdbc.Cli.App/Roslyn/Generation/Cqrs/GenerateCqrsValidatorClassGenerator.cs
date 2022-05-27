using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Cqrs
{
    public static class GenerateCqrsValidatorClassGenerator
    {


        public static async Task GenerateCqrsValidatorClass(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.CqrsInputClassName.ToValidator();
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
                    .AddUsingStatement("FluentValidation")
                    .AddUsingAertssenFrameworkValidationInfra()
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddBaseClass($"FluentValidationValidator<{service.GenerationContext.ActionInfo.CqrsInputClassName}>")
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }
        }
    }
}