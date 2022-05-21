using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Entity
{
    public static class EntityUnitTestGenerator
    {
        public static async Task GenerateEntityUnitTest(this GenerationService service)
        {
            var className = service.GenerationContext.EntityName.ToSpecification();
            var subfolders = new[] { "Domain", "Model" };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

            entity = ClassDeclarationSyntaxBuilder.ForBaseSpecification(className, entityNamespace)
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                .AddUsingAertssenFrameworkAuditModel()
                .Build();

            await service.FileHelperService.WriteFile(fullFilename, entity);

            entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.AssertionFailedTestMethod("Verify_domain_model_action"), fullFilename);

        }
    }
}