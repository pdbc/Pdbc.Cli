using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Cqrs.UnitTests
{
    public static class GenerateCqrsFactoryUnitTestClassGenerator
    {
        public static async Task GenerateCqrsFactoryUnitTestClass(this GenerationService service)
        {
            var className = service.GenerationContext.CqrsFactoryClassName.ToSpecification();

            var subfolders = new[] { "Core", "CQRS", service.GenerationContext.PluralEntityName, service.GenerationContext.ActionName };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

            entity = new ClassDeclarationSyntaxBuilder()
                .WithName(className)
                .ForNamespace(entityNamespace)
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddUsingAertssenFrameworkValidationInfra()
                .AddUsingStatement("System.Collections.Generic")
                .AddUnitTestUsingStatement()
                .AddUsingAertssenFrameworkAuditModel()
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForCoreCqrs())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddTestFixtureAttribute(true)
                .AddBaseClass($"{service.GenerationContext.CqrsFactoryClassName.ToContextSpecification()}")
                .Build();

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_factory_logic_executed")
                    .AddTestAttribute(true)
                    .ThrowsNewNotImplementedException(),
                fullFilename);


            
        }
    }
}