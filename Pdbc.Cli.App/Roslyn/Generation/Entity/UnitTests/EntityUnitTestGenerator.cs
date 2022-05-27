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
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

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

            entity = await service.AppendUsingStatement(entity, service.GenerationContext.GetNamespaceForDomainModel(), fullFilename);
            entity = await service.AppendUsingStatement(entity, service.GenerationContext.GetNamespaceForDomainModelHelpers(), fullFilename);

            entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.AssertionFailedTestMethod("Verify_domain_model_action"), fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                .WithName("Verify_property_change_is_audited")
                .AddParameter("string", "propertyName")
                .AddParameter("Boolean", "expectation")
                .AddAttribute($"TestCase(nameof({service.GenerationContext.EntityName}.Id), true)")
                .AddStatement($"var entity = new {service.GenerationContext.EntityName.ToTestDataBuilder()}().Build();")
                .AddStatement($"entity.ShouldAuditPropertyChangeFor(propertyName).ShouldBeEqualTo(expectation);"), fullFilename);


        }
    }
}