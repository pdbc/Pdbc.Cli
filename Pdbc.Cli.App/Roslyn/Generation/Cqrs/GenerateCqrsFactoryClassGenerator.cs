using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Cqrs
{
    public static class GenerateCqrsFactoryClassGenerator
    {
        public static async Task GenerateCqrsFactoryClass(this GenerationService service)
        {
            var className = service.GenerationContext.CqrsFactoryClassName;
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
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddBaseClass($"IFactory<{service.GenerationContext.ActionDtoInterface},{service.GenerationContext.EntityName}>")
                    .Build();
            }

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Create")
                    .WithReturnType(service.GenerationContext.EntityName)
                    .AddParameter(service.GenerationContext.ActionDtoClass.ToInterface(), "model")
                    .ThrowsNewNotImplementedException(),
                fullFilename);
        }
    }
}