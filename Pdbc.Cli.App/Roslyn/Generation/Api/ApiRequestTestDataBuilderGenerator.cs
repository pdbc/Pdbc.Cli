using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;

namespace Pdbc.Cli.App.Roslyn.Generation.Api
{
    public static class ApiRequestTestDataBuilderGenerator
    {
        public static async Task GenerateApiRequestClassTestDataBuilder(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.ApiRequestClassName.ToTestDataBuilder();
            var subfolders = new[] { "Api", service.GenerationContext.PluralEntityName };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Tests.Helpers");
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
                .AddUsingStatement(service.GenerationContext.GetNamespaceForServices())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForRequests())
                .AddAertssenFrameworkCoreUsingStatements()
                .AddUnitTestUsingStatement()
                .AddBaseClass($"{service.GenerationContext.ActionInfo.ApiRequestClassName.ToBuilder()}")
                .Build();

            await service.FileHelperService.WriteFile(fullFilename, entity);

            if (service.GenerationContext.ActionInfo.RequiresActionDto)
            {
                entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                        .AddStatement(
                            $"{service.GenerationContext.EntityName} = new {service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToTestDataBuilder()}();"),
                    fullFilename);

            }

            if (service.GenerationContext.ActionInfo.IsGetAction || service.GenerationContext.ActionInfo.IsDeleteAction)
            {
                entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                        .AddStatement($"Id = UnitTestValueGenerator.GenerateRandomNumber();"),
                    fullFilename);

            }
        }
    }
}