using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;

namespace Pdbc.Cli.App.Roslyn.Generation.IntegrationTests
{
    public static class ActionSpecificServiceIntegrationTestGenerator
    {

        public static async Task GenerateActionSpecificIntegrationTest(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.ActionOperationName.ToIntegrationTest();
            var subfolders = new[] {"IntegrationTests", service.GenerationContext.PluralEntityName};

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Integration.Tests");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddBaseClass(
                        $"{service.GenerationContext.ServiceContractName.ToTest()}<{service.GenerationContext.ActionInfo.ApiResponseClassNameOverride}>")
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForData())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForServices())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForRequests())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForApiTestDataBuilders())
                    .AddAertssenFrameworkContractUsingStatements()
                    .AddUnitTestUsingStatement()
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await service.Save(entity, new VariableDeclarationSyntaxBuilder()
                    .WithName($"_request")
                    .ForType($"{service.GenerationContext.ActionInfo.ApiRequestClassName}")
                , fullFilename);


            entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter(service.GenerationContext.ServiceContractName.ToInterface(), "service")
                    .AddParameter(service.GenerationContext.ApplicationName.ToDbContext(), "context")
                    .AddBaseParameter("service")
                    .AddBaseParameter("context")
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Setup")
                    .IsOverride(true)
                    .AddStatement(new StatementSyntaxBuilder(
                        $"_request = new {service.GenerationContext.ActionInfo.ApiRequestClassName.ToTestDataBuilder()}();"))
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Cleanup")
                    .IsOverride(true)
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("PerformAction")
                    .WithReturnType(service.GenerationContext.ActionInfo.ApiResponseClassNameOverride)
                    .IsOverride(true)
                    .AddStatement(new StatementSyntaxBuilder(
                        $"return Service.{service.GenerationContext.ActionInfo.ActionOperationName}(_request).GetAwaiter().GetResult();"))
                ,
                fullFilename);

            if (service.GenerationContext.ActionInfo.IsListAction)
            {
                entity = await service.GenerateVerifyResponseNotImplementedExceptionMethod(entity, fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsGetAction)
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("VerifyResponse")
                        .AddParameter(service.GenerationContext.ActionInfo.ApiResponseClassName, "response")
                        .IsOverride(true)
                        .AddStatement(
                            new StatementSyntaxBuilder($"response.Notifications?.HasErrors().ShouldBeFalse();"))
                        .AddStatement(
                            new StatementSyntaxBuilder(
                                $"response.{service.GenerationContext.EntityName}.ShouldNotBeNull();"))
                    //
                    ,
                    fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsDeleteAction)
            {
                entity = await service.GenerateVerifyResponseNotImplementedExceptionMethod(entity, fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsStoreAction)
            {
                entity = await service.GenerateVerifyResponseNotImplementedExceptionMethod(entity, fullFilename);
            }

        }
    }
}