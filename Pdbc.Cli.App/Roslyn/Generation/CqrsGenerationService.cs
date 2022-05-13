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
            //if (!_generationContext.IsListAction)
            if (_generationContext.ShouldGenerateCqrsOutputClass)
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

            if (_generationContext.IsGetAction || _generationContext.IsDeleteAction)
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
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_repository")
                    .ForType(_generationContext.EntityName.ToRepositoryInterface())
                    .WithIsReadonly(true), fullFilename);
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_projectionService")
                    .ForType("IProjectionService")
                    .WithIsReadonly(true), fullFilename);


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
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_repository")
                    .ForType(_generationContext.EntityName.ToRepositoryInterface())
                    .WithIsReadonly(true), fullFilename);
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_mapper")
                    .ForType("IMapper")
                    .WithIsReadonly(true), fullFilename);

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
                        .AddStatement(new AssignmentSyntaxBuilder("var item", $"await _repository.GetByIdAsync(request.Id)"))
                        .AddStatement(new AssignmentSyntaxBuilder("var mappedItem", $"_mapper.Map<{_generationContext.DataDtoClass}>(item)"))
                        //.AddStatement(new ReturnStatementSyntaxBuilder().WithName($"new {_generationContext.CqrsOutputClassName}()"+"{"+$"{_generationContext.EntityName} = mappedItem"+"}")),
                        .AddStatement(new ReturnStatementSyntaxBuilder().WithObjectCreationExpression(new ObjectCreationExpressionSyntaxBuilder($"{_generationContext.CqrsOutputClassName}").AddAssignementStatement($"{_generationContext.EntityName}", "mappedItem"))),

                    fullFilename);
            } 
            else if (_generationContext.IsDeleteAction)
            {
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_repository")
                    .ForType(_generationContext.EntityName.ToRepositoryInterface())
                    .WithIsReadonly(true), fullFilename);

                entity = await Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                        .AddParameter(_generationContext.EntityName.ToRepositoryInterface(), "repository")
                        .AddStatement(new AssignmentSyntaxBuilder("_repository", "repository"))
                    ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Handle")
                        .Async()
                        .WithReturnType($"Task<{outputCqrsOutputClassName}>")
                        .AddParameter(_generationContext.CqrsInputClassName, "request")
                        .AddParameter("CancellationToken", "cancellationToken")
                        .AddStatement(new AssignmentSyntaxBuilder("var item", $"await _repository.GetByIdAsync(request.Id)"))
                        .AddStatement(new StatementSyntaxBuilder("_repository.Delete(item);"))
                        //.AddStatement(new ReturnStatementSyntaxBuilder().WithName($"new {_generationContext.CqrsOutputClassName}()"+"{"+$"{_generationContext.EntityName} = mappedItem"+"}")),
                        .AddStatement(new ReturnStatementSyntaxBuilder().WithName("await Nothing.AtAllAsync()")),

                    fullFilename);
            }
            else if (_generationContext.IsStoreAction)
            {
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_factory")
                    .ForType($"IFactory<{_generationContext.ActionDtoInterface}, {_generationContext.EntityName}>")
                    .WithIsReadonly(true), fullFilename);
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_changesHandler")
                    .ForType($"IChangesHandler<{_generationContext.ActionDtoInterface}, {_generationContext.EntityName}>")
                    .WithIsReadonly(true), fullFilename);
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_repository")
                    .ForType(_generationContext.EntityName.ToRepositoryInterface())
                    .WithIsReadonly(true), fullFilename);
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_mapper")
                    .ForType("IMapper")
                    .WithIsReadonly(true), fullFilename);
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_dbContextService")
                    .ForType($"I{_generationContext.Configuration.ApplicationName}DbContextService")
                    .WithIsReadonly(true), fullFilename);

                entity = await Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                        .AddParameter($"IFactory<{_generationContext.ActionDtoInterface}, {_generationContext.EntityName}>", "factory")
                        .AddParameter($"IChangesHandler<{_generationContext.ActionDtoInterface}, {_generationContext.EntityName}>", "changesHandler")
                        .AddParameter(_generationContext.EntityName.ToRepositoryInterface(), "repository")
                        .AddParameter($"IMapper", "mapper")
                        .AddParameter($"I{_generationContext.Configuration.ApplicationName}DbContextService", "dbContextService")
                        .AddStatement(new AssignmentSyntaxBuilder("_factory", "factory"))
                        .AddStatement(new AssignmentSyntaxBuilder("_changesHandler", "changesHandler"))
                        .AddStatement(new AssignmentSyntaxBuilder("_repository", "repository"))
                        .AddStatement(new AssignmentSyntaxBuilder("_mapper", "mapper"))
                        .AddStatement(new AssignmentSyntaxBuilder("_dbContextService", "dbContextService"))
                    ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Handle")
                        .Async()
                        .WithReturnType($"Task<{outputCqrsOutputClassName}>")
                        .AddParameter(_generationContext.CqrsInputClassName, "request")
                        .AddParameter("CancellationToken", "cancellationToken")
                        .AddStatement(new StatementSyntaxBuilder($"{_generationContext.EntityName} entity;"))
                        .AddStatement(new StatementSyntaxBuilder($"if (request.{_generationContext.EntityName}.Id.HasValue) "))
                        .AddStatement(new BlockStatementSyntaxBuilder()
                            .AddStatement(new StatementSyntaxBuilder($"   entity = _repository.GetById(request.{_generationContext.EntityName}.Id.Value);"))
                            .AddStatement(new StatementSyntaxBuilder($"   _changesHandler.ApplyChanges(entity, request.{_generationContext.EntityName});"))
                            .AddStatement(new StatementSyntaxBuilder($"   _repository.Update(entity);"))
                        )
                        .AddStatement(new StatementSyntaxBuilder($" else "))
                        .AddStatement(new BlockStatementSyntaxBuilder()
                            .AddStatement(new StatementSyntaxBuilder($"   entity = _factory.Create(request.{_generationContext.EntityName});"))
                            .AddStatement(new StatementSyntaxBuilder($"   _repository.Insert(entity);"))
                        )
                        .AddStatement(new StatementSyntaxBuilder($""))
                        .AddStatement(new StatementSyntaxBuilder($"await _dbContextService.SaveChangesAsync(cancellationToken);"))
                        .AddStatement(new StatementSyntaxBuilder($" return new {_generationContext.CqrsOutputClassName} {{ {_generationContext.EntityName} = _mapper.Map<{_generationContext.DataDtoClass}>(entity) }};"))
,

                    fullFilename);
            } else
            {
                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Handle")
                        .WithReturnType($"Task<{outputCqrsOutputClassName}>")
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

            bool isAbstract = _generationContext.IsStoreAction;

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
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrs())
                .AddUsingStatement(_generationContext.GetNamespaceForDto())
                .AddUsingStatement(_generationContext.GetNamespaceForData())
                .AddUsingStatement(_generationContext.GetNamespaceForDataRepositories())
                .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrsTestDataBuilders())
                .AddUsingStatement(_generationContext.GetNamespaceForDomainTestDataBuilders())
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

            }
            else if (_generationContext.IsDeleteAction)
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Command")
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


                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Establish_context")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement("base.Establish_context();"))
                        .AddStatement(new AssignmentSyntaxBuilder("Command", $"new {_generationContext.CqrsInputClassName.ToTestDataBuilder()}().Build()"))
                        .AddStatement(new AssignmentSyntaxBuilder("CancellationToken", $"new CancellationToken()"))
                        .AddStatement(new AssignmentSyntaxBuilder("Item", $"new {_generationContext.EntityName.ToTestDataBuilder()}().Build()"))
                        .AddStatement(new StatementSyntaxBuilder().AddStatement($"Repository.Stub(x => x.GetByIdAsync(Command.Id)).ReturnsAsync(Item);")),
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Because")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement("SUT.Handle(Command, CancellationToken).GetAwaiter().GetResult();"))
                    ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_repository_called_to_load_item")
                        .WithModifier(SyntaxKind.PublicKeyword)
                        .AddTestAttribute(true)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement("Repository.AssertWasCalled(x => x.GetByIdAsync(Command.Id));"))
                    ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_repository_called_to_delete_the_item")
                        .WithModifier(SyntaxKind.PublicKeyword)
                        .AddTestAttribute(true)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement($"Repository.AssertWasCalled(x => x.Delete(Item));"))
                    ,
                    fullFilename);

            }
            else if (_generationContext.IsStoreAction)
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Command")
                    .ForType(_generationContext.CqrsInputClassName), fullFilename);
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Repository")
                    .ForType($"{_generationContext.EntityName.ToRepositoryInterface()}")
                    .WithDependencyType(_generationContext.EntityName.ToRepositoryInterface()), fullFilename);

                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("Factory")
                    .ForType($"IFactory<{_generationContext.ActionDtoInterface}, {_generationContext.EntityName}>")
                    .WithDependencyType($"IFactory<{_generationContext.ActionDtoInterface}, {_generationContext.EntityName}>"), fullFilename);
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("ChangesHandler")
                    .ForType($"IChangesHandler<{_generationContext.ActionDtoInterface}, {_generationContext.EntityName}>")
                    .WithDependencyType($"IChangesHandler<{_generationContext.ActionDtoInterface}, {_generationContext.EntityName}>"), fullFilename);
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName($"{_generationContext.Configuration.ApplicationName}DbContextService")
                    .ForType($"I{_generationContext.Configuration.ApplicationName}DbContextService")
                    .WithDependencyType($"I{_generationContext.Configuration.ApplicationName}DbContextService"), fullFilename);


                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("CreatedItem")
                    .ForType(_generationContext.EntityName), fullFilename);
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("LoadedItem")
                    .ForType(_generationContext.EntityName), fullFilename);

                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder()
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .WithName("CancellationToken")
                    .ForType("CancellationToken"), fullFilename);
             
                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Establish_context")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement("base.Establish_context();"))
                        .AddStatement(new StatementSyntaxBuilder("Command = CreateCommand();"))
                        .AddStatement(new StatementSyntaxBuilder("CreatedItem = GetCreatedItem();"))
                        .AddStatement(new StatementSyntaxBuilder("LoadedItem = GetLoadedItem();"))

                        .AddStatement(new StatementSyntaxBuilder($"Factory.Stub(x => x.Create(Command.{_generationContext.EntityName})).Return(CreatedItem);"))
                        .AddStatement(new StatementSyntaxBuilder($"if (Command.{_generationContext.EntityName}.Id.HasValue)"))
                        .AddStatement(new BlockStatementSyntaxBuilder()
                            .AddStatement(new StatementSyntaxBuilder($"Repository.Stub(x => x.GetById(Command.{_generationContext.EntityName}.Id.Value)).Return(LoadedItem);"))
                        )
                    ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("CreateCommand")
                        .WithReturnType(_generationContext.CqrsInputClassName)
                        .IsVirtual(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder($"return new {_generationContext.CqrsInputClassName.ToTestDataBuilder()}().Build();"))
                    ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("GetCreatedItem")
                        .WithReturnType(_generationContext.EntityName)
                        .IsAbstract(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                    ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("GetLoadedItem")
                        .WithReturnType(_generationContext.EntityName)
                        .IsAbstract(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                    ,
                    fullFilename);

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Because")
                        .IsOverride(true)
                        .WithModifier(SyntaxKind.ProtectedKeyword)
                        .AddStatement(new StatementSyntaxBuilder().AddStatement("SUT.Handle(Command, CancellationToken).GetAwaiter().GetResult();"))
                    ,
                    fullFilename);

                
                await GenerateCqrsHandlerUnitTestClassForStoreCreate();
                await GenerateCqrsHandlerUnitTestClassForStoreUpdate();

                //entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                //        .WithName("Verify_repository_called_to_load_item")
                //        .WithModifier(SyntaxKind.PublicKeyword)
                //        .AddTestAttribute(true)
                //        .AddStatement(new StatementSyntaxBuilder().AddStatement("Repository.AssertWasCalled(x => x.GetByIdAsync(Command.Id));"))
                //    ,
                //    fullFilename);

                //entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                //        .WithName("Verify_repository_called_to_delete_the_item")
                //        .WithModifier(SyntaxKind.PublicKeyword)
                //        .AddTestAttribute(true)
                //        .AddStatement(new StatementSyntaxBuilder().AddStatement($"Repository.AssertWasCalled(x => x.Delete(Item));"))
                //    ,
                //    fullFilename);

            } else 
            {

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_handler_executed")
                        .AddTestAttribute(true)
                        .ThrowsNewNotImplementedException(),
                    fullFilename);
            }
        }

        public async Task GenerateCqrsHandlerUnitTestClassForStoreCreate()
        {
            var className = $"Create{_generationContext.CqrsHandlerClassName}".ToSpecification();
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
                .AddUsingStatement("Moq")
                .AddUnitTestUsingStatement()
                .AddUsingAertssenFrameworkAuditModel()
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrs())
                .AddUsingStatement(_generationContext.GetNamespaceForDto())
                .AddUsingStatement(_generationContext.GetNamespaceForData())
                .AddUsingStatement(_generationContext.GetNamespaceForDataRepositories())
                .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrsTestDataBuilders())
                .AddUsingStatement(_generationContext.GetNamespaceForDomainTestDataBuilders())
                .AddUsingStatement("AutoMapper")
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddUsingAertssenFrameworkServices()
                .AddTestFixtureAttribute(true)
                .AddBaseClass($"{_generationContext.CqrsHandlerClassName.ToSpecification()}")
                .Build();

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("GetCreatedItem")
                    .IsOverride(true)
                    .WithReturnType(_generationContext.EntityName)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement($"return new {_generationContext.EntityName.ToTestDataBuilder()}();"))
                ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("GetLoadedItem")
                    .IsOverride(true)
                    .WithReturnType(_generationContext.EntityName)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement($"return null;"))
                ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_factory_called_to_setup_item")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement($"Factory.AssertWasCalled(x => x.Create(Command.{_generationContext.EntityName}));"))
                ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_repository_called_to_insert_the_object")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement($"Repository.AssertWasCalled(x => x.Insert(CreatedItem));"))
                ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_dbcontextservice_called_to_savechanges")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement($"{_generationContext.Configuration.ApplicationName}DbContextService.AssertWasCalled(x => x.SaveChangesAsync(CancellationToken));"))
                ,
                fullFilename);

            await _fileHelperService.WriteFile(fullFilename, entity);
        }
        public async Task GenerateCqrsHandlerUnitTestClassForStoreUpdate()
        {
            var className = $"Update{_generationContext.CqrsHandlerClassName}".ToSpecification();
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
                .AddUsingStatement("Moq")
                .AddUnitTestUsingStatement()
                .AddUsingAertssenFrameworkAuditModel()
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrs())
                .AddUsingStatement(_generationContext.GetNamespaceForDto())
                .AddUsingStatement(_generationContext.GetNamespaceForData())
                .AddUsingStatement(_generationContext.GetNamespaceForDataRepositories())
                .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrsTestDataBuilders())
                .AddUsingStatement(_generationContext.GetNamespaceForDomainTestDataBuilders())
                .AddUsingStatement("AutoMapper")
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddUsingAertssenFrameworkServices()
                .AddTestFixtureAttribute(true)
                .AddBaseClass($"{_generationContext.CqrsHandlerClassName.ToSpecification()}")
                .Build();

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("CreateCommand")
                    .IsOverride(true)
                    .WithReturnType(_generationContext.CqrsInputClassName)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement($"return new {_generationContext.CqrsInputClassName.ToTestDataBuilder()}();"))
                ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                   .WithName("GetCreatedItem")
                   .IsOverride(true)
                   .WithReturnType(_generationContext.EntityName)
                   .WithModifier(SyntaxKind.ProtectedKeyword)
                   .AddStatement(new StatementSyntaxBuilder().AddStatement($"return null;"))
                ,
               fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("GetLoadedItem")
                    .IsOverride(true)
                    .WithReturnType(_generationContext.EntityName)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement($"return new {_generationContext.EntityName.ToTestDataBuilder()}();"))
                ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_repository_called_to_load_the_entity")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement($"Repository.AssertWasCalled(x => x.GetById(Command.{_generationContext.EntityName}.Id.Value));;"))
                ,
                fullFilename);


            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_changes_handler_called_to_update_the_object")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement($"ChangesHandler.AssertWasCalled(x => x.ApplyChanges(LoadedItem, Command.{_generationContext.EntityName}));"))
                ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_repository_called_to_update_the_object")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement($"Repository.AssertWasCalled(x => x.Update(LoadedItem));"))
                ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_dbcontextservice_called_to_savechanges")
                    .WithModifier(SyntaxKind.PublicKeyword)
                    .AddTestAttribute(true)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement($"{_generationContext.Configuration.ApplicationName}DbContextService.AssertWasCalled(x => x.SaveChangesAsync(CancellationToken));"))
                ,
                fullFilename);

            await _fileHelperService.WriteFile(fullFilename, entity);
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
                .AddBaseClass($"{_generationContext.CqrsFactoryClassName.ToContextSpecification()}")
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
                .AddBaseClass($"{_generationContext.CqrsChangesHandlerClassName.ToContextSpecification()}")
                .Build();

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_changes_handler_logic_executed")
                    .AddTestAttribute(true)
                    .ThrowsNewNotImplementedException(),
                fullFilename);
        }

    }
}