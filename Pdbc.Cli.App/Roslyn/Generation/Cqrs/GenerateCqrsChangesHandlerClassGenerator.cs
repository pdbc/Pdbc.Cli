using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Cqrs
{
    public static class GenerateCqrsChangesHandlerClassGenerator
    {
        public static async Task GenerateCqrsChangesHandlerClass(this GenerationService service)
        {
            var className = service.GenerationContext.CqrsChangesHandlerClassName;
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
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddUsingAertssenFrameworkValidationInfra()
                    .AddUsingStatement("FluentValidation")
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddBaseClass($"IChangesHandler<{service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToInterface()},{service.GenerationContext.EntityName}>")
                    .Build();
            }

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("ApplyChanges")
                    .AddParameter(service.GenerationContext.EntityName, "entity")
                    .AddParameter(service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToInterface(), "model")
                    .ThrowsNewNotImplementedException(),
                fullFilename);
        }



    }
}