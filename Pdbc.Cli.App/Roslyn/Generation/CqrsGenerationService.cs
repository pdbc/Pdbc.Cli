using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Model.Items;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation
{
    public class CqrsGenerationService : BaseGenerationService
    {
        public CqrsGenerationService(RoslynSolutionContext roslynSolutionContext,
            FileHelperService fileHelperService,
            GenerationContext context
        ) : base(roslynSolutionContext, fileHelperService, context)
        {
        }

        public async Task Generate()
        {
            await GenerateCqrsInputClass();
            await GenerateCqrsInputClassTestDataBuilder();

            // TODO quick action here?
            if (!_generationContext.IsListAction)
            {
                await GenerateCqrsOutputClass();
            }
            
            await GenerateCqrsHandlerClass();
            await GenerateCqrsHandlerUnitTestClass();

            await GenerateCqrsValidatorClass();
            await GenerateCqrsValidatorUnitTestClass();

            if (_generationContext.RequiresFactory)
            {
                await GenerateCqrsFactoryClass();
                await GenerateCqrsFactoryUnitTestClass();
            }

            if (_generationContext.RequiresChangesHandler)
            {
                await GenerateCqrsChangesHandlerClass();
                await GenerateCqrsChangesHandlerUnitTestClass();
            }
            

        }

        private async Task GenerateCqrsInputClassTestDataBuilder()
        {
            var className = _generationContext.CqrsInputClassName.ToTestDataBuilder();
            var subfolders = new[] { "CQRS", _generationContext.PluralEntityName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Tests.Helpers");
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
                .AddUsingStatement("System")
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrs())
                .AddAertssenFrameworkCoreUsingStatements()
                .AddUnitTestUsingStatement()
                .AddBaseClass($"{_generationContext.CqrsInputClassName.ToBuilder()}")
                .Build();

            await _fileHelperService.WriteFile(fullFilename, entity);

        }

        private String[] GetSubFolders()
        {
            return new[]
            {
                "CQRS",
                _generationContext.PluralEntityName,
                _generationContext.ActionName
            };
        }


        public async Task GenerateCqrsInputClass()
        {
            var className = _generationContext.CqrsInputClassName;
            var subfolders = GetSubFolders();

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                    .AddUsingStatement(_generationContext.GetNamespaceForDto())
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddBaseClass($"I{_generationContext.CqrsInputType}<{_generationContext.GetCqrsOutputClassNameBasedOnAction()}>")
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }

            if (_generationContext.RequiresActionDto)
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName(_generationContext.EntityName).ForType(_generationContext.ActionDtoInterface), fullFilename);
            }

            if (_generationContext.IsGetAction)
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName("Id").ForType("long"), fullFilename);
            }
        }

        public async Task GenerateCqrsOutputClass()
        {
            var className = _generationContext.CqrsOutputClassName;
            var subfolders = GetSubFolders();

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);


            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(_generationContext.GetNamespaceForDto())
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);

            }

            if (_generationContext.RequiresDataDto)
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName(_generationContext.EntityName).ForType(_generationContext.DataDtoInterface), fullFilename);
            }
        }
        
        public async Task GenerateCqrsHandlerClass()
        {
            var className = _generationContext.CqrsHandlerClassName;
            var subfolders = GetSubFolders();

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);

            var outputCqrsOutputClassName = _generationContext.GetCqrsOutputClassNameBasedOnAction();

            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement("System.Threading")
                    .AddUsingStatement("System.Threading.Tasks")
                    .AddUsingStatement("AutoMapper")
                    .AddUsingStatement("MediatR")
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                    .AddUsingStatement(_generationContext.GetNamespaceForData())
                    .AddUsingStatement(_generationContext.GetNamespaceForDataRepositories())
                    .AddUsingStatement(_generationContext.GetNamespaceForDto())
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddUsingAertssenFrameworkServices()
                    .AddBaseClass($"IRequestHandler<{_generationContext.CqrsInputClassName}, {outputCqrsOutputClassName}>")
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }


            // List Query Handler
            if (_generationContext.IsListAction) 
            {

                //var items = _assetRepository.GetAll();
                //var mappedAssets = _projectionService.Project<Domain.Model.Asset, AssetDataDto>(items);
                //return mappedAssets;

                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_repository").ForType(_generationContext.EntityName.ToRepositoryInterface()), fullFilename);
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_projectionService").ForType("IProjectionService"), fullFilename);


                entity = await Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                        .AddParameter(_generationContext.EntityName.ToRepositoryInterface(), "repository")
                        .AddParameter("IProjectionService", "projectionService")
                        .AddStatement(new AssignmentSyntaxBuilder("_repository", "repository"))
                        .AddStatement(new AssignmentSyntaxBuilder("_projectionService", "projectionService"))
                        ,
                    fullFilename);


                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Handle")
                        .Async()
                        .WithReturnType($"Task<{outputCqrsOutputClassName}>")
                        .AddParameter(_generationContext.CqrsInputClassName, "request")
                        .AddParameter("CancellationToken", "cancellationToken")
                        .AddStatement(new AssignmentSyntaxBuilder("var items", $"_repository.GetAll()"))
                        .AddStatement(new AssignmentSyntaxBuilder("var mappedItems", $"_projectionService.Project<{_generationContext.EntityName},{_generationContext.DataDtoClass}>(items)"))
                        .AddStatement(new ReturnStatementSyntaxBuilder().WithName("mappedItems")),
                    fullFilename);


            }
            else if (_generationContext.IsGetAction)
            {
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_repository").ForType(_generationContext.EntityName.ToRepositoryInterface()), fullFilename);
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_mapper").ForType("IMapper"), fullFilename);

                entity = await Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                        .AddParameter(_generationContext.EntityName.ToRepositoryInterface(), "repository")
                        .AddParameter("IMapper", "mapper")
                        .AddStatement(new AssignmentSyntaxBuilder("_repository", "repository"))
                        .AddStatement(new AssignmentSyntaxBuilder("_mapper", "mapper"))
                    ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Handle")
                        .Async()
                        .WithReturnType($"Task<{outputCqrsOutputClassName}>")
                        .AddParameter(_generationContext.CqrsInputClassName, "request")
                        .AddParameter("CancellationToken", "cancellationToken")
                        .AddStatement(new AssignmentSyntaxBuilder("var item", $"_repository.GetByIdAsync(request.Id)"))
                        .AddStatement(new AssignmentSyntaxBuilder("var mappedItem", $"_mapper.Map<{_generationContext.DataDtoClass}>(item)"))
                        //.AddStatement(new ReturnStatementSyntaxBuilder().WithName($"new {_generationContext.CqrsOutputClassName}()"+"{"+$"{_generationContext.EntityName} = mappedItem"+"}")),
                        .AddStatement(new ReturnStatementSyntaxBuilder().WithObjectCreationExpression(new ObjectCreationExpressionSyntaxBuilder($"{_generationContext.CqrsOutputClassName}").AddAssignementStatement($"{_generationContext.EntityName}", "mappedItem"))),
                    //TODO Add Result here...
                    fullFilename);
            }
            else
            {
                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Handle")
                        .WithReturnType($"Task<{_generationContext.CqrsOutputClassName}>")
                        .AddParameter(_generationContext.CqrsInputClassName, "request")
                        .AddParameter("CancellationToken", "cancellationToken")
                        .ThrowsNewNotImplementedException(),
                    fullFilename);
            }



        }

        public async Task GenerateCqrsHandlerUnitTestClass()
        {
            var className = _generationContext.CqrsHandlerClassName.ToSpecification();
            var subfolders = GetSubFoldersForUnitTest();

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
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
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrs())
                .AddUsingStatement(_generationContext.GetNamespaceForDto())
                .AddUsingStatement(_generationContext.GetNamespaceForDataRepositories())
                .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrsTestDataBuilders())
                .AddUsingStatement("AutoMapper")
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddUsingAertssenFrameworkServices()
                .AddTestFixtureAttribute(true)
                .AddBaseClass($"{_generationContext.CqrsHandlerClassName.ToContextSpecification()}")
                .Build();


            if (_generationContext.IsListAction)
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Query")
                    .ForType(_generationContext.CqrsInputClassName), fullFilename);
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("CancellationToken")
                    .ForType("CancellationToken"), fullFilename);
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Items")
                    .ForType($"IQueryable<{_generationContext.EntityName}>"), fullFilename);
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Repository")
                    .ForType($"{_generationContext.EntityName.ToRepositoryInterface()}")
                    .WithDependencyType(_generationContext.EntityName.ToRepositoryInterface()), fullFilename);
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("ProjectionService")
                    .ForType($"IProjectionService")
                    .WithDependencyType("IProjectionService"), fullFilename);



                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Establish_context")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement("base.Establish_context();"))
                        .AddStatement(new AssignmentSyntaxBuilder("Query", $"new {_generationContext.CqrsInputClassName.ToTestDataBuilder()}().Build()"))
                        .AddStatement(new AssignmentSyntaxBuilder("CancellationToken", $"new CancellationToken()"))
                        .AddStatement(new AssignmentSyntaxBuilder("Items", $"new List<{_generationContext.EntityName}>().AsQueryable()"))
                        .AddStatement(new StatementSyntaxBuilder().AddStatement($"Repository.Stub(x => x.GetAll()).Returns(Items);")),
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Because")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement("SUT.Handle(Query, CancellationToken).GetAwaiter().GetResult();"))
                        ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_repository_called_to_load_items")
                        .WithModifier(SyntaxKind.PublicKeyword)
                        .AddTestAttribute(true)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement("Repository.AssertWasCalled(x => x.GetAll());"))
                    ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_projection_service_called_to_map_data_to_dto_result")
                        .WithModifier(SyntaxKind.PublicKeyword)
                        .AddTestAttribute(true)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement($"ProjectionService.AssertWasCalled(x => x.Project<{_generationContext.EntityName}, {_generationContext.DataDtoClass}>(Items));"))
                    ,
                    fullFilename);

            }
            else if (_generationContext.IsGetAction)
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Query")
                    .ForType(_generationContext.CqrsInputClassName), fullFilename);
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("CancellationToken")
                    .ForType("CancellationToken"), fullFilename);
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Item")
                    .ForType($"{_generationContext.EntityName}"), fullFilename);
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Repository")
                    .ForType($"{_generationContext.EntityName.ToRepositoryInterface()}")
                    .WithDependencyType(_generationContext.EntityName.ToRepositoryInterface()), fullFilename);
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Mapper")
                    .ForType($"IMapper")
                    .WithDependencyType("IMapper"), fullFilename);



                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Establish_context")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement("base.Establish_context();"))
                        .AddStatement(new AssignmentSyntaxBuilder("Query", $"new {_generationContext.CqrsInputClassName.ToTestDataBuilder()}().Build()"))
                        .AddStatement(new AssignmentSyntaxBuilder("CancellationToken", $"new CancellationToken()"))
                        .AddStatement(new AssignmentSyntaxBuilder("Item", $"new {_generationContext.EntityName.ToTestDataBuilder()}().Build()"))
                        .AddStatement(new StatementSyntaxBuilder().AddStatement($"Repository.Stub(x => x.GetByIdAsync(Query.Id)).ReturnsAsync(Item);")),
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Because")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement("SUT.Handle(Query, CancellationToken).GetAwaiter().GetResult();"))
                        ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_repository_called_to_load_item")
                        .WithModifier(SyntaxKind.PublicKeyword)
                        .AddTestAttribute(true)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement("Repository.AssertWasCalled(x => x.GetByIdAsync(Query.Id));"))
                    ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_mapper_called_to_map_data_to_dto_result")
                        .WithModifier(SyntaxKind.PublicKeyword)
                        .AddTestAttribute(true)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement($"Mapper.AssertWasCalled(x => x.Map<{_generationContext.DataDtoClass}>(Item));"))
                    ,
                    fullFilename);

            } else 
            {

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_handler_executed")
                        .AddTestAttribute(true)
                        .ThrowsNewNotImplementedException(),
                    fullFilename);
            }
        }


        public async Task GenerateCqrsValidatorClass()
        {
            var className = _generationContext.CqrsValidatorClassName;
            var subfolders = GetSubFolders();

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement("FluentValidation")
                    .AddUsingAertssenFrameworkValidationInfra()
                    .AddUsingStatement(_generationContext.GetNamespaceForDto())
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddBaseClass($"FluentValidationValidator<{_generationContext.CqrsInputClassName}>")
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }
        }

        public async Task GenerateCqrsFactoryClass()
        {
            var className = _generationContext.CqrsFactoryClassName;
            var subfolders = GetSubFolders();

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                    .AddUsingStatement(_generationContext.GetNamespaceForDto())
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddBaseClass($"IFactory<{_generationContext.ActionDtoInterface},{_generationContext.EntityName}>")
                    .Build();
            }

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Create")
                    .WithReturnType(_generationContext.EntityName)
                    .AddParameter(_generationContext.ActionDtoClass.ToInterface(), "model")
                    .ThrowsNewNotImplementedException(),
                fullFilename);
        }

        public async Task GenerateCqrsChangesHandlerClass()
        {
            var className = _generationContext.CqrsChangesHandlerClassName;
            var subfolders = GetSubFolders();

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddUsingAertssenFrameworkValidationInfra()
                    .AddUsingStatement("FluentValidation")
                    .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                    .AddUsingStatement(_generationContext.GetNamespaceForDto())
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddBaseClass($"IChangesHandler<{_generationContext.ActionDtoInterface},{_generationContext.EntityName}>")
                    .Build();
            }

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("ApplyChanges")
                    .AddParameter(_generationContext.EntityName, "entity")
                    .AddParameter(_generationContext.ActionDtoClass.ToInterface(), "model")
                    .ThrowsNewNotImplementedException(),
                fullFilename);
        }



        private string[] GetSubFoldersForUnitTest()
        {
            return new[] { "Core", "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName };
        }



        public async Task GenerateCqrsValidatorUnitTestClass()
        {
            var className = _generationContext.CqrsValidatorClassName.ToSpecification();

            var subfolders = new[] { "Core", "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
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
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrs())
                .AddUsingStatement(_generationContext.GetNamespaceForDto())
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddTestFixtureAttribute(true)
                .AddBaseClass($"{_generationContext.CqrsValidatorClassName.ToContextSpecification()}")
                .Build();

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_validator_logic_executed")
                    .AddTestAttribute(true)
                    .ThrowsNewNotImplementedException(),
                fullFilename);

        }

        public async Task GenerateCqrsFactoryUnitTestClass()
        {
            var className = _generationContext.CqrsFactoryClassName.ToSpecification();

            var subfolders = new[] { "Core", "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
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
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrs())
                .AddUsingStatement(_generationContext.GetNamespaceForDto())
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddTestFixtureAttribute(true)
                .AddBaseClass($"{_generationContext.CqrsFactoryClassName.ToContextSpecification()}>")
                .Build();

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_factory_logic_executed")
                    .AddTestAttribute(true)
                    .ThrowsNewNotImplementedException(),
                fullFilename);


            
        }

        public async Task GenerateCqrsChangesHandlerUnitTestClass()
        {
            var className = _generationContext.CqrsChangesHandlerClassName.ToSpecification();

            var subfolders = new[] { "Core", "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
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
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrs())
                .AddUsingStatement(_generationContext.GetNamespaceForDto())
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddTestFixtureAttribute(true)
                .AddBaseClass($"{_generationContext.CqrsChangesHandlerClassName.ToContextSpecification()}>")
                .Build();

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_changes_handler_logic_executed")
                    .AddTestAttribute(true)
                    .ThrowsNewNotImplementedException(),
                fullFilename);
        }

    }
}