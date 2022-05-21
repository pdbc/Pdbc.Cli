using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Entity
{
    public static class EntityRepositorClassGenerator
    {

        public static async Task GenerateRepositoryClass(this GenerationService service)
        {
            var className = service.GenerationContext.EntityName.ToRepository();
            var subfolders = new[] { "Repositories" };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Data");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

            entity = new ClassDeclarationSyntaxBuilder()
                .WithName(className)
                .ForNamespace(entityNamespace)
                .AddUsingAertssenFrameworkRepositories()
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                .AddUsingAertssenFrameworkAuditModel()
                .AddBaseClass($"EntityFrameworkRepository<{service.GenerationContext.EntityName}>")
                .AddBaseClass(service.GenerationContext.EntityName.ToRepositoryInterface())
                .Build();

            await service.FileHelperService.WriteFile(fullFilename, entity);


            entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter(service.GenerationContext.ApplicationName.ToDbContext(), "context")
                    .AddBaseParameter("context")
                ,
                fullFilename);

        }
    }
}