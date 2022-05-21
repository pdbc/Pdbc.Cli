using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation.IntegrationTests
{
    public static class IntegrationTestsGenerationService 
    {
        public static async Task GenerateBaseServiceIntegrationTest(this GenerationService service)
        {
            var className = service.GenerationContext.ServiceContractName.ToTest();
            var subfolders = new[] {"IntegrationTests", service.GenerationContext.PluralEntityName};
            
            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Integration.Tests");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName($"{className}<TResult>")
                    .ForNamespace(entityNamespace)
                    .IsAbstract(true)
                    .AddBaseClass($"{service.GenerationContext.BaseIntegrationTestClass}<TResult> where TResult : AertssenResponse")
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForData())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForServices())
                    .AddAertssenFrameworkContractUsingStatements()
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await service.Save(entity, new VariableDeclarationSyntaxBuilder()
                .WithName($"Service")
                .ForType($"{service.GenerationContext.ServiceContractName.ToInterface()}")
                .WithModifier(SyntaxKind.ProtectedKeyword), fullFilename);


            entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter(service.GenerationContext.ServiceContractName.ToInterface(), "service")
                    .AddParameter(service.GenerationContext.ApplicationName.ToDbContext(), "context")
                    .AddBaseParameter("context")
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new AssignmentSyntaxBuilder("Service", "service"))
                ,
                fullFilename);
        }

        public static async Task GenerateActionSpecificIntegrationTest(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.ActionOperationName.ToIntegrationTest();
            var subfolders = new[] { "IntegrationTests", service.GenerationContext.PluralEntityName };

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
                    .AddBaseClass($"{service.GenerationContext.ServiceContractName.ToTest()}<{service.GenerationContext.GetApiOutputClassNameBasedOnAction()}>")
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
                .ForType($"{service.GenerationContext.ActionInfo.RequestInputClassName}")
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
                    .AddStatement(new StatementSyntaxBuilder($"_request = new {service.GenerationContext.ActionInfo.RequestInputClassName.ToTestDataBuilder()}();"))
                  ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Cleanup")
                    .IsOverride(true)
                ,
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("PerformAction")
                    .WithReturnType(service.GenerationContext.GetApiOutputClassNameBasedOnAction())
                    .IsOverride(true)
                    .AddStatement(new StatementSyntaxBuilder($"return Service.{service.GenerationContext.ActionInfo.ActionOperationName}(_request).GetAwaiter().GetResult();"))
                ,
                fullFilename);

            if (service.GenerationContext.ActionInfo.IsListAction)
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("VerifyResponse")
                        .AddParameter(service.GenerationContext.GetApiOutputClassNameBasedOnAction(), "response")
                        .IsOverride(true)
                        .AddStatement(
                            new StatementSyntaxBuilder($"response.Notifications?.HasErrors().ShouldBeFalse();"))
                        .AddStatement(
                            new StatementSyntaxBuilder($"throw new NotImplementedException();"))
                    //
                    ,
                    fullFilename);
            }
            else if(service.GenerationContext.ActionInfo.IsGetAction)
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("VerifyResponse")
                        .AddParameter(service.GenerationContext.GetApiOutputClassNameBasedOnAction(), "response")
                        .IsOverride(true)
                        .AddStatement(
                            new StatementSyntaxBuilder($"response.Notifications?.HasErrors().ShouldBeFalse();"))
                        .AddStatement(
                            new StatementSyntaxBuilder($"response.{service.GenerationContext.EntityName}.ShouldNotBeNull();"))
                    //
                    ,
                    fullFilename);
            } else if (service.GenerationContext.ActionInfo.IsDeleteAction)
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("VerifyResponse")
                        .AddParameter(service.GenerationContext.GetApiOutputClassNameBasedOnAction(), "response")
                        .IsOverride(true)
                        .AddStatement(
                            new StatementSyntaxBuilder($"response.Notifications?.HasErrors().ShouldBeFalse();"))
                        .AddStatement(
                            new StatementSyntaxBuilder($"throw new NotImplementedException();"))
                    //
                    ,
                    fullFilename);
            }

        }

        public static async Task GenerateWebApiIntegrationTest(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.ActionOperationName.ToSpecification();
            var subfolders = new[] { service.GenerationContext.PluralEntityName };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Api");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddBaseClass($"{service.GenerationContext.BaseWebApiIntegrationTestClass}")
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForData())
                    .AddUsingStatement(service.GenerationContext.GeNameForServiceAgents())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForIntegrationTests())
                    .AddUsingStatement("Microsoft.Extensions.DependencyInjection")
                    .AddUsingStatement("Microsoft.Extensions.Logging")
                    .AddUsingAertssenFrameworkInfraTests()
                    .AddAertssenFrameworkContractUsingStatements()
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("CreateIntegrationTest")
                    .WithReturnType("IIntegrationTest")
                    .IsOverride(true)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder($"var service = new {service.GenerationContext.WebApiServiceContractName}(CreateWebApiProxy, ServiceProvider.GetService<ILogger<{service.GenerationContext.WebApiServiceContractName}>>());"))
                    .AddStatement(new StatementSyntaxBuilder($"return new {service.GenerationContext.ActionInfo.ActionOperationName.ToIntegrationTest()}(service, Context);"))
                ,
                fullFilename);
        }

        public static async Task GenerateCqrsIntegrationTest(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.ActionOperationName.ToSpecification();
            var subfolders = new[] { "Cqrs", service.GenerationContext.PluralEntityName };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Core");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                //using Locations.Integration.Tests.IntegrationTests.Routes;
                //using Locations.Services.Cqrs.Services.Routes;
                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddBaseClass($"{service.GenerationContext.BaseCqrsIntegrationTestClass}")
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForData())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForIntegrationTests())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForCqrsServices())
                    .AddUsingStatement("Microsoft.Extensions.DependencyInjection")
                    .AddUsingStatement("Microsoft.Extensions.Logging")
                    .AddUsingAertssenFrameworkInfraTests()
                    .AddAertssenFrameworkContractUsingStatements()
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            
            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("CreateIntegrationTest")
                    .WithReturnType("IIntegrationTest")
                    .IsOverride(true)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder($"var service = ServiceProvider.GetRequiredService<{service.GenerationContext.CqrsServiceContractName.ToInterface()}>();"))
                    .AddStatement(new StatementSyntaxBuilder($"return new {service.GenerationContext.ActionInfo.ActionOperationName.ToIntegrationTest()}(service, Context);"))
                  ,
                fullFilename);
        }
        
    }
}
