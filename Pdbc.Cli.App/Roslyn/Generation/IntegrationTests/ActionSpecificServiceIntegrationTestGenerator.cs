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
                        $"{service.GenerationContext.ActionInfo.ServiceContractName.ToTest()}<{service.GenerationContext.ActionInfo.ApiResponseClassNameOverride}>")
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForData())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForServices())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForRequests())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForApiTestDataBuilders())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
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
                    .AddParameter(service.GenerationContext.ActionInfo.ServiceContractName.ToInterface(), "service")
                    .AddParameter(service.GenerationContext.ApplicationName.ToDbContext(), "context")
                    .AddBaseParameter("service")
                    .AddBaseParameter("context")
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
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                        .WithName($"Entity")
                        .ForType($"{service.GenerationContext.EntityName}")
                    , fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Setup")
                        .IsOverride(true)
                        .AddStatement($"Entity = TestCaseService.Setup{service.GenerationContext.EntityName}();")
                        .AddStatement($"_request = new {service.GenerationContext.ActionInfo.ApiRequestClassName.ToTestDataBuilder()}();"),
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Cleanup")
                        .IsOverride(true)
                        .AddStatement($"DbContext.SafeRemoveEntityForIntegrationTest(Entity);")
                    ,
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("VerifyResponse")
                        .AddParameter(service.GenerationContext.ActionInfo.ApiResponseClassName, "response")
                        .IsOverride(true)
                        .AddStatement($"response.Notifications?.HasErrors().ShouldBeFalse();")
                        .AddStatement($"response.Items.Any().ShouldBeTrue();"),
                    fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsGetAction)
            {
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                        .WithName($"Entity")
                        .ForType($"{service.GenerationContext.EntityName}")
                    , fullFilename);


                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Setup")
                        .IsOverride(true)
                        .AddStatement($"Entity = TestCaseService.Setup{service.GenerationContext.EntityName}();")
                        .AddStatement($"_request = new {service.GenerationContext.ActionInfo.ApiRequestClassName.ToTestDataBuilder()}().WithId(Entity.Id);"),
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Cleanup")
                        .IsOverride(true)
                        .AddStatement($"DbContext.SafeRemoveEntityForIntegrationTest(Entity);")
                    ,
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("VerifyResponse")
                        .AddParameter(service.GenerationContext.ActionInfo.ApiResponseClassName, "response")
                        .IsOverride(true)
                        .AddStatement($"response.Notifications?.HasErrors().ShouldBeFalse();")
                        .AddStatement($"response.{service.GenerationContext.EntityName}.ShouldNotBeNull();"),
                    fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsDeleteAction)
            {
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                        .WithName($"Entity")
                        .ForType($"{service.GenerationContext.EntityName}")
                    , fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Setup")
                        .IsOverride(true)
                        .AddStatement($"Entity = TestCaseService.Setup{service.GenerationContext.EntityName}();")
                        .AddStatement($"_request = new {service.GenerationContext.ActionInfo.ApiRequestClassName.ToTestDataBuilder()}().WithId(Entity.Id);"),
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Cleanup")
                        .IsOverride(true),
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("CheckActionSucceeded")
                        .IsOverride(true)
                        .AddStatement($" DbContext.Set<{service.GenerationContext.EntityName}>().Find(Entity.Id).ShouldBeNull();"),
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("VerifyResponse")
                        .AddParameter(service.GenerationContext.ActionInfo.ApiResponseClassNameOverride, "response")
                        .IsOverride(true)
                        .AddStatement($"response.Notifications?.HasErrors().ShouldBeFalse();"),
                    fullFilename);
                
            }
            else if (service.GenerationContext.ActionInfo.IsStoreAction)
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Setup")
                        .IsOverride(true)
                        .AddStatement($"_request = new {service.GenerationContext.ActionInfo.ApiRequestClassName.ToTestDataBuilder()}();"),
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Cleanup")
                        .IsOverride(true)
                    ,
                    fullFilename);
                entity = await service.GenerateVerifyResponseNotImplementedExceptionMethod(entity, fullFilename);
            }
            else
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Setup")
                        .IsOverride(true)
                        .AddStatement($"_request = new {service.GenerationContext.ActionInfo.ApiRequestClassName.ToTestDataBuilder()}();"),
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Cleanup")
                        .IsOverride(true),
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("VerifyResponse")
                        .AddParameter(service.GenerationContext.ActionInfo.ApiResponseClassNameOverride, "response")
                        .IsOverride(true)
                        .AddStatement($"response.Notifications?.HasErrors().ShouldBeFalse();"),
                    fullFilename);
            }

        }
    }
}