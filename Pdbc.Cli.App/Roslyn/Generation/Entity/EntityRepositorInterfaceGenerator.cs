using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Entity
{
    public static class EntityRepositorInterfaceGenerator
    {
        public static async Task GenerateRepositoryInterface(this GenerationService service)
        {
            var className = service.GenerationContext.EntityName.ToRepositoryInterface();
            var subfolders = new[] {"Repositories"};

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Data");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetInterfaceByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

            entity = new InterfaceDeclarationSyntaxBuilder()
                .WithName(className)
                .ForNamespace(entityNamespace)
                .AddUsingAertssenFrameworkAuditModel()
                .AddUsingAertssenFrameworkRepositories()
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                .AddBaseClass($"IEntityRepository<{service.GenerationContext.EntityName}>")
                .Build();

            await service.FileHelperService.WriteFile(fullFilename, entity);
        }
    }
}
