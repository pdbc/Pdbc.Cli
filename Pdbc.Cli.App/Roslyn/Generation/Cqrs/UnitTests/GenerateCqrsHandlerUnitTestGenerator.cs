using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;

namespace Pdbc.Cli.App.Roslyn.Generation.Cqrs.UnitTests
{
    public static class GenerateCqrsHandlerUnitTestGenerator
    {

        public static async Task GenerateCqrsHandlerUnitTestClass(this GenerationService service)
        {
            var className = service.GenerationContext.CqrsHandlerClassName.ToSpecification();
            var subfolders = new[] {"Core", "CQRS", service.GenerationContext.PluralEntityName, service.GenerationContext.ActionName};

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            bool isAbstract = service.GenerationContext.ActionInfo.IsStoreAction;

            var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

            entity = new ClassDeclarationSyntaxBuilder()
                .WithName(className)
                .ForNamespace(entityNamespace)
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddUsingAertssenFrameworkValidationInfra()
                .AddUsingStatement("System.Collections.Generic")
                .AddUsingStatement("Moq")
                .IsAbstract(isAbstract)
                .AddUnitTestUsingStatement()
                .AddUsingAertssenFrameworkAuditModel()
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForCoreCqrs())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForData())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDataRepositories())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForCoreCqrsTestDataBuilders())
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainTestDataBuilders())
                .AddUsingStatement("AutoMapper")
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddUsingAertssenFrameworkServices()
                .AddTestFixtureAttribute(true)
                .AddBaseClass($"{service.GenerationContext.CqrsHandlerClassName.ToContextSpecification()}")
                .Build();


            if (service.GenerationContext.ActionInfo.IsListAction)
            {
                entity = await service.GenerateQueryProperty(entity, fullFilename);
                entity = await service.GenerateCancellationTokenProperty(entity, fullFilename);

                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Items")
                    .ForType($"IQueryable<{service.GenerationContext.EntityName}>"), fullFilename);

                entity = await service.GenerateUnitTestRepositoryProperty(entity, fullFilename);
                entity = await service.GenerateUnitTestProjectionService(entity, fullFilename);
                
                entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.UnitTestEstablishContext()
                        .AddStatement(new AssignmentSyntaxBuilder("Query", $"new {service.GenerationContext.ActionInfo.CqrsInputClassName.ToTestDataBuilder()}().Build()"))
                        .AddStatement(new AssignmentSyntaxBuilder("CancellationToken", $"new CancellationToken()"))
                        .AddStatement(new AssignmentSyntaxBuilder("Items", $"new List<{service.GenerationContext.EntityName}>().AsQueryable()"))
                        .AddStatement(new StatementSyntaxBuilder($"Repository.Stub(x => x.GetAll()).Returns(Items);")),
                    fullFilename);

                entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.UnitTestBecause()
                        .AddStatement(new StatementSyntaxBuilder("SUT.Handle(Query, CancellationToken).GetAwaiter().GetResult();")),
                    fullFilename);

                entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.UnitTestMethod("Verify_repository_called_to_load_items")
                       .AddStatement(new StatementSyntaxBuilder("Repository.AssertWasCalled(x => x.GetAll());"))
                    ,
                    fullFilename);

                entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.UnitTestMethod("Verify_projection_service_called_to_map_data_to_dto_result")
                      .AddStatement(new StatementSyntaxBuilder($"ProjectionService.AssertWasCalled(x => x.Project<{service.GenerationContext.EntityName}, {service.GenerationContext.EntityName.ToDataDto()}>(Items));")),
                    fullFilename);

            }
            else if (service.GenerationContext.ActionInfo.IsGetAction)
            {
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Query")
                    .ForType(service.GenerationContext.ActionInfo.CqrsInputClassName), fullFilename);
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("CancellationToken")
                    .ForType("CancellationToken"), fullFilename);
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Item")
                    .ForType($"{service.GenerationContext.EntityName}"), fullFilename);
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Repository")
                    .ForType($"{service.GenerationContext.EntityName.ToRepositoryInterface()}")
                    .WithDependencyType(service.GenerationContext.EntityName.ToRepositoryInterface()), fullFilename);
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Mapper")
                    .ForType($"IMapper")
                    .WithDependencyType("IMapper"), fullFilename);



                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Establish_context")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder("base.Establish_context();"))
                        .AddStatement(new AssignmentSyntaxBuilder("Query",
                            $"new {service.GenerationContext.ActionInfo.CqrsInputClassName.ToTestDataBuilder()}().Build()"))
                        .AddStatement(new AssignmentSyntaxBuilder("CancellationToken", $"new CancellationToken()"))
                        .AddStatement(new AssignmentSyntaxBuilder("Item",
                            $"new {service.GenerationContext.EntityName.ToTestDataBuilder()}().Build()"))
                        .AddStatement(new StatementSyntaxBuilder(
                            $"Repository.Stub(x => x.GetByIdAsync(Query.Id)).ReturnsAsync(Item);")),
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Because")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(
                            new StatementSyntaxBuilder(
                                "SUT.Handle(Query, CancellationToken).GetAwaiter().GetResult();"))
                    ,
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_repository_called_to_load_item")
                        .WithModifier(SyntaxKind.PublicKeyword)
                        .AddTestAttribute(true)
                        .AddStatement(
                            new StatementSyntaxBuilder(
                                "Repository.AssertWasCalled(x => x.GetByIdAsync(Query.Id));"))
                    ,
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_mapper_called_to_map_data_to_dto_result")
                        .WithModifier(SyntaxKind.PublicKeyword)
                        .AddTestAttribute(true)
                        .AddStatement(new StatementSyntaxBuilder(
                            $"Mapper.AssertWasCalled(x => x.Map<{service.GenerationContext.EntityName.ToDataDto()}>(Item));"))
                    ,
                    fullFilename);

            }
            else if (service.GenerationContext.ActionInfo.IsDeleteAction)
            {
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Command")
                    .ForType(service.GenerationContext.ActionInfo.CqrsInputClassName), fullFilename);
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("CancellationToken")
                    .ForType("CancellationToken"), fullFilename);
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Item")
                    .ForType($"{service.GenerationContext.EntityName}"), fullFilename);
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Repository")
                    .ForType($"{service.GenerationContext.EntityName.ToRepositoryInterface()}")
                    .WithDependencyType(service.GenerationContext.EntityName.ToRepositoryInterface()), fullFilename);


                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Establish_context")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder("base.Establish_context();"))
                        .AddStatement(new AssignmentSyntaxBuilder("Command",
                            $"new {service.GenerationContext.ActionInfo.CqrsInputClassName.ToTestDataBuilder()}().Build()"))
                        .AddStatement(new AssignmentSyntaxBuilder("CancellationToken", $"new CancellationToken()"))
                        .AddStatement(new AssignmentSyntaxBuilder("Item",
                            $"new {service.GenerationContext.EntityName.ToTestDataBuilder()}().Build()"))
                        .AddStatement(new StatementSyntaxBuilder(
                            $"Repository.Stub(x => x.GetByIdAsync(Command.Id)).ReturnsAsync(Item);")),
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Because")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder(
                            "SUT.Handle(Command, CancellationToken).GetAwaiter().GetResult();"))
                    ,
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_repository_called_to_load_item")
                        .WithModifier(SyntaxKind.PublicKeyword)
                        .AddTestAttribute(true)
                        .AddStatement(
                            new StatementSyntaxBuilder(
                                "Repository.AssertWasCalled(x => x.GetByIdAsync(Command.Id));"))
                    ,
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_repository_called_to_delete_the_item")
                        .WithModifier(SyntaxKind.PublicKeyword)
                        .AddTestAttribute(true)
                        .AddStatement(
                            new StatementSyntaxBuilder(
                                $"Repository.AssertWasCalled(x => x.Delete(Item));"))
                    ,
                    fullFilename);

            }
            else if (service.GenerationContext.ActionInfo.IsStoreAction)
            {
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Command")
                    .ForType(service.GenerationContext.ActionInfo.CqrsInputClassName), fullFilename);
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Repository")
                    .ForType($"{service.GenerationContext.EntityName.ToRepositoryInterface()}")
                    .WithDependencyType(service.GenerationContext.EntityName.ToRepositoryInterface()), fullFilename);

                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .WithName("Factory")
                        .ForType($"IFactory<{service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToInterface()}, {service.GenerationContext.EntityName}>")
                        .WithDependencyType(
                            $"IFactory<{service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToInterface()}, {service.GenerationContext.EntityName}>"),
                    fullFilename);
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .WithName("ChangesHandler")
                        .ForType(
                            $"IChangesHandler<{service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToInterface()}, {service.GenerationContext.EntityName}>")
                        .WithDependencyType(
                            $"IChangesHandler<{service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToInterface()}, {service.GenerationContext.EntityName}>"),
                    fullFilename);
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName($"{service.GenerationContext.ApplicationName}DbContextService")
                    .ForType($"I{service.GenerationContext.ApplicationName}DbContextService")
                    .WithDependencyType($"I{service.GenerationContext.ApplicationName}DbContextService"), fullFilename);


                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("CreatedItem")
                    .ForType(service.GenerationContext.EntityName), fullFilename);
                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("LoadedItem")
                    .ForType(service.GenerationContext.EntityName), fullFilename);

                entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("CancellationToken")
                    .ForType("CancellationToken"), fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Establish_context")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder("base.Establish_context();"))
                        .AddStatement(new StatementSyntaxBuilder("Command = CreateCommand();"))
                        .AddStatement(new StatementSyntaxBuilder("CreatedItem = GetCreatedItem();"))
                        .AddStatement(new StatementSyntaxBuilder("LoadedItem = GetLoadedItem();"))

                        .AddStatement(new StatementSyntaxBuilder(
                            $"Factory.Stub(x => x.Create(Command.{service.GenerationContext.EntityName})).Return(CreatedItem);"))
                        .AddStatement(
                            new StatementSyntaxBuilder($"if (Command.{service.GenerationContext.EntityName}.Id.HasValue)"))
                        .AddStatement(new BlockStatementSyntaxBuilder()
                            .AddStatement(new StatementSyntaxBuilder(
                                $"Repository.Stub(x => x.GetById(Command.{service.GenerationContext.EntityName}.Id.Value)).Return(LoadedItem);"))
                        )
                    ,
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("CreateCommand")
                        .WithReturnType(service.GenerationContext.ActionInfo.CqrsInputClassName)
                        .IsVirtual(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder(
                            $"return new {service.GenerationContext.ActionInfo.CqrsInputClassName.ToTestDataBuilder()}().Build();"))
                    ,
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("GetCreatedItem")
                        .WithReturnType(service.GenerationContext.EntityName)
                        .IsAbstract(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                    ,
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("GetLoadedItem")
                        .WithReturnType(service.GenerationContext.EntityName)
                        .IsAbstract(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                    ,
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Because")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder(
                            "SUT.Handle(Command, CancellationToken).GetAwaiter().GetResult();"))
                    ,
                    fullFilename);


                await service.GenerateCqrsHandlerUnitTestClassForStoreCreate();
                await service.GenerateCqrsHandlerUnitTestClassForStoreUpdate();

                //entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                //        .WithName("Verify_repository_called_to_load_item")
                //        .WithModifier(SyntaxKind.PublicKeyword)
                //        .AddTestAttribute(true)
                //        .AddStatement(new StatementSyntaxBuilder("Repository.AssertWasCalled(x => x.GetByIdAsync(Command.Id));"))
                //    ,
                //    fullFilename);

                //entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                //        .WithName("Verify_repository_called_to_delete_the_item")
                //        .WithModifier(SyntaxKind.PublicKeyword)
                //        .AddTestAttribute(true)
                //        .AddStatement(new StatementSyntaxBuilder($"Repository.AssertWasCalled(x => x.Delete(Item));"))
                //    ,
                //    fullFilename);

            }
            else
            {

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_handler_executed")
                        .AddTestAttribute(true)
                        .ThrowsNewNotImplementedException(),
                    fullFilename);
            }
        }
    }
}