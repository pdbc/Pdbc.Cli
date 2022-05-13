using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Model.Items;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation
{
    public class IntegrationTestsGenerationService : BaseGenerationService
    {
        public IntegrationTestsGenerationService(RoslynSolutionContext roslynSolutionContext,
            FileHelperService fileHelperService,
            GenerationContext context
        ) : base(roslynSolutionContext, fileHelperService, context)
        {
        }

        public async Task Generate()
        {
            await GenerateBaseServiceIntegrationTest();
            await GenerateActionSpecificIntegrationTest();
            await GenerateCqrsIntegrationTest();
            await GenerateWebApiIntegrationTest();
            //await GenerateWebApiServiceContractClass();
            //await GenerateCqrsServiceContractInterface();
            //await GenerateCqrsServiceContractClass();
        }




        public async Task GenerateBaseServiceIntegrationTest()
        {
            var className = _generationContext.ServiceContractName.ToTest();
            var subfolders = new[] {"IntegrationTests", _generationContext.PluralEntityName};
            
            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Integration.Tests");
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
                    .AddBaseClass($"{_generationContext.BaseIntegrationTestClass}<TResult> where TResult : AertssenResponse")
                    .AddUsingStatement(_generationContext.GetNamespaceForData())
                    .AddUsingStatement(_generationContext.GetNamespaceForServices())
                    .AddAertssenFrameworkContractUsingStatements()
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await Save(entity, new VariableDeclarationSyntaxBuilder()
                .WithName($"Service")
                .ForType($"{_generationContext.ServiceContractName.ToInterface()}")
                .WithModifier(SyntaxKind.ProtectedKeyword), fullFilename);


            entity = await Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter(_generationContext.ServiceContractName.ToInterface(), "service")
                    .AddParameter(_generationContext.DbContextName, "context")
                    .AddBaseParameter("context")
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new AssignmentSyntaxBuilder("Service", "service"))
                ,
                fullFilename);
        }

        private async Task GenerateActionSpecificIntegrationTest()
        {
            var className = _generationContext.ActionOperationName.ToIntegrationTest();
            var subfolders = new[] { "IntegrationTests", _generationContext.PluralEntityName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Integration.Tests");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddBaseClass($"{_generationContext.ServiceContractName.ToTest()}<{_generationContext.GetApiOutputClassNameBasedOnAction()}>")
                    .AddUsingStatement(_generationContext.GetNamespaceForData())
                    .AddUsingStatement(_generationContext.GetNamespaceForServices())
                    .AddUsingStatement(_generationContext.GetNamespaceForRequests())
                    .AddUsingStatement(_generationContext.GetNamespaceForApiTestDataBuilders())
                    .AddAertssenFrameworkContractUsingStatements()
                    .AddUnitTestUsingStatement()
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await Save(entity, new VariableDeclarationSyntaxBuilder()
                .WithName($"_request")
                .ForType($"{_generationContext.RequestInputClassName}")
                , fullFilename);


            entity = await Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter(_generationContext.ServiceContractName.ToInterface(), "service")
                    .AddParameter(_generationContext.DbContextName, "context")
                    .AddBaseParameter("service")
                    .AddBaseParameter("context")
                ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Setup")
                    .IsOverride(true)
                    .AddStatement(new StatementSyntaxBuilder($"_request = new {_generationContext.RequestInputClassName.ToTestDataBuilder()}();"))
                  ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Cleanup")
                    .IsOverride(true)
                ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("PerformAction")
                    .WithReturnType(_generationContext.GetApiOutputClassNameBasedOnAction())
                    .IsOverride(true)
                    .AddStatement(new StatementSyntaxBuilder($"return Service.{_generationContext.ActionOperationName}(_request).GetAwaiter().GetResult();"))
                ,
                fullFilename);

            if (_generationContext.IsListAction)
            {
                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("VerifyResponse")
                        .AddParameter(_generationContext.GetApiOutputClassNameBasedOnAction(), "response")
                        .IsOverride(true)
                        .AddStatement(
                            new StatementSyntaxBuilder($"response.Notifications?.HasErrors().ShouldBeFalse();"))
                        .AddStatement(
                            new StatementSyntaxBuilder($"throw new NotImplementedException();"))
                    //
                    ,
                    fullFilename);
            }
            else if(_generationContext.IsGetAction)
            {
                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("VerifyResponse")
                        .AddParameter(_generationContext.GetApiOutputClassNameBasedOnAction(), "response")
                        .IsOverride(true)
                        .AddStatement(
                            new StatementSyntaxBuilder($"response.Notifications?.HasErrors().ShouldBeFalse();"))
                        .AddStatement(
                            new StatementSyntaxBuilder($"response.{_generationContext.EntityName}.ShouldNotBeNull();"))
                    //
                    ,
                    fullFilename);
            } else if (_generationContext.IsDeleteAction)
            {
                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("VerifyResponse")
                        .AddParameter(_generationContext.GetApiOutputClassNameBasedOnAction(), "response")
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

        private async Task GenerateWebApiIntegrationTest()
        {
            var className = _generationContext.ActionOperationName.ToSpecification();
            var subfolders = new[] { _generationContext.PluralEntityName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Api");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddBaseClass($"{_generationContext.BaseWebApiIntegrationTestClass}")
                    .AddUsingStatement(_generationContext.GetNamespaceForData())
                    .AddUsingStatement(_generationContext.GeNameForServiceAgents())
                    .AddUsingStatement(_generationContext.GetNamespaceForIntegrationTests())
                    .AddUsingStatement("Microsoft.Extensions.DependencyInjection")
                    .AddUsingStatement("Microsoft.Extensions.Logging")
                    .AddUsingAertssenFrameworkInfraTests()
                    .AddAertssenFrameworkContractUsingStatements()
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("CreateIntegrationTest")
                    .WithReturnType("IIntegrationTest")
                    .IsOverride(true)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder($"var service = new {_generationContext.WebApiServiceContractName}(CreateWebApiProxy, ServiceProvider.GetService<ILogger<{_generationContext.WebApiServiceContractName}>>());"))
                    .AddStatement(new StatementSyntaxBuilder($"return new {_generationContext.ActionOperationName.ToIntegrationTest()}(service, Context);"))
                ,
                fullFilename);
        }

        private async Task GenerateCqrsIntegrationTest()
        {
            var className = _generationContext.ActionOperationName.ToSpecification();
            var subfolders = new[] { "Cqrs", _generationContext.PluralEntityName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Core");
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
                    .AddBaseClass($"{_generationContext.BaseCqrsIntegrationTestClass}")
                    .AddUsingStatement(_generationContext.GetNamespaceForData())
                    .AddUsingStatement(_generationContext.GetNamespaceForIntegrationTests())
                    .AddUsingStatement(_generationContext.GetNamespaceForCqrsServices())
                    .AddUsingStatement("Microsoft.Extensions.DependencyInjection")
                    .AddUsingStatement("Microsoft.Extensions.Logging")
                    .AddUsingAertssenFrameworkInfraTests()
                    .AddAertssenFrameworkContractUsingStatements()
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }

            
            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("CreateIntegrationTest")
                    .WithReturnType("IIntegrationTest")
                    .IsOverride(true)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder($"var service = ServiceProvider.GetRequiredService<{_generationContext.CqrsServiceContractName.ToInterface()}>();"))
                    .AddStatement(new StatementSyntaxBuilder($"return new {_generationContext.ActionOperationName.ToIntegrationTest()}(service, Context);"))
                  ,
                fullFilename);
        }
        
    }
}
