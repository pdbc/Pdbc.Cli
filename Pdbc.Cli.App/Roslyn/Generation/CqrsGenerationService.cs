using System;
using System.Collections.Generic;
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

            await GenerateCqrsOutputClass();
            
            await GenerateCqrsHandlerClass();
            await GenerateCqrsHandlerUnitTestClass();

            await GenerateCqrsValidatorClass();
            await GenerateCqrsValidatorUnitTestClass();

            if (_generationContext.RequiresFactory())
            {
                await GenerateCqrsFactoryClass();
                await GenerateCqrsFactoryUnitTestClass();
            }

            if (_generationContext.RequiresChangesHandler())
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
                .AddUsingStatement(roslynProjectContext.GetNamespaceForDomainModel())
                .AddUsingStatement(roslynProjectContext.GetNamespaceForCoreCqrs(_generationContext.PluralEntityName, _generationContext.ActionName))
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
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDomainModel())
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddBaseClass($"I{_generationContext.CqrsInputType}<{_generationContext.GetCqrsOutputClassNameBasedOnAction()}>")
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }

            if (_generationContext.RequiresActionDto())
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName(_generationContext.EntityName).ForType(_generationContext.ActionDtoInterface), fullFilename);
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
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);

            }

            if (_generationContext.RequiresDataDto())
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
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDomainModel())
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForData())
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDataRepositories())
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddUsingAertssenFrameworkServices()
                    .AddBaseClass($"IRequestHandler<{_generationContext.CqrsInputClassName}, {outputCqrsOutputClassName}>")
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }


            // List Query Handler
            if (_generationContext.IsListAction()) 
            {

                //var items = _assetRepository.GetAll();
                //var mappedAssets = _projectionService.Project<Domain.Model.Asset, AssetDataDto>(items);
                //return mappedAssets;

                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_repository").ForType(_generationContext.EntityName.ToRepositoryInterface()), fullFilename);
                entity = await Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_projectionService").ForType("IProjectionService"), fullFilename);


                entity = await Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                        .AddParameter(_generationContext.EntityName.ToRepositoryInterface(), "repository")
                        .AddParameter("IProjectionService", "projectionService")
                        .AddStatement(new AssignmentSyntaxBuilder().WithVariableName("_repository").WithParameterName("repository"))
                        .AddStatement(new AssignmentSyntaxBuilder().WithVariableName("_projectionService").WithParameterName("projectionService"))
                        ,
                    fullFilename);


                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Handle")
                        .Async()
                        .WithReturnType($"Task<{outputCqrsOutputClassName}>")
                        .AddParameter(_generationContext.CqrsInputClassName, "request")
                        .AddParameter("CancellationToken", "cancellationToken")
                        .AddStatement(new AssignmentSyntaxBuilder().WithVariableName("var items").WithParameterName($"_repository.GetAll()"))
                        .AddStatement(new AssignmentSyntaxBuilder().WithVariableName("var mappedItems").WithParameterName($"_projectionService.Project<{_generationContext.EntityName},{_generationContext.DataDtoClass}>(items)"))
                        .AddStatement(new ReturnStatementSyntaxBuilder().WithName("mappedItems")),
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


            // Variable declarations
            //entity = await _roslynGenerator.AppendClassVariable(fullFilename, entity,
            //    new PropertyItem($"IFactory<{_generationContext.ActionDtoInterface}, {_generationContext.EntityName}>", "_factory"));
            //entity = await _roslynGenerator.AppendClassVariable(fullFilename, entity,
            //    new PropertyItem($"IChangesHandler<{_generationContext.ActionDtoInterface}, {_generationContext.EntityName}>", "_changesHandler"));
            //entity = await _roslynGenerator.AppendClassVariable(fullFilename, entity,
            //    new PropertyItem($"{_generationContext.EntityName.ToRepositoryInterface()}", "_repository"));


            // Constructor
            //public Task<AddressStoreResult> Handle(AddressStoreCommand request, CancellationToken cancellationToken)
            //{
            //    throw new NotImplementedException();
            //}


            // Handle Method
            //entity = await _roslynGenerator.AppendPublicMethodNotImplemented(fullFilename, entity, new MethodItem($"", "Handle"),
            //    new List<PropertyItem>()
            //    {
            //        new PropertyItem(_generationContext.CqrsInputClassName, "request"),
            //        new PropertyItem("CancellationToken", "cancellationToken")
            //    });

            //entity = await _roslynGenerator.AppendCqrsHandlerStoreMethod(fullFilename, entity, _generationContext.StandardActionInfo);
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
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
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
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDomainModel())
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
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
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDomainModel())
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
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
                .AddUsingStatement(roslynProjectContext.GetNamespaceForDomainModel())
                .AddUsingStatement(roslynProjectContext.GetNamespaceForCoreCqrs(_generationContext.PluralEntityName, _generationContext.ActionName))
                .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
                .AddUsingStatement(roslynProjectContext.GetNamespaceForDataRepositories())
                .AddUsingStatement(roslynProjectContext.GetNamespaceForCoreCqrsTestDataBuilders(_generationContext.PluralEntityName))
                .AddUsingAertssenFrameworkCqrsInfra()
                .AddUsingAertssenFrameworkServices()
                .AddTestFixtureAttribute(true)
                .AddBaseClass($"{_generationContext.CqrsHandlerClassName.ToContextSpecification()}")
                .Build();


            if (_generationContext.IsListAction())
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
                        .AddStatement(new AssignmentSyntaxBuilder().WithVariableName("Query").WithParameterName($"new {_generationContext.CqrsInputClassName.ToTestDataBuilder()}().Build()"))
                        .AddStatement(new AssignmentSyntaxBuilder().WithVariableName("CancellationToken").WithParameterName($"new CancellationToken()"))
                        .AddStatement(new AssignmentSyntaxBuilder().WithVariableName("Items").WithParameterName($"new List<{_generationContext.EntityName}>().AsQueryable()"))
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
                //[Test]
                //public void Verify_repository_called_to_load_assets()
                //{
                //    Repository.AssertWasCalled(x => x.GetAll());
                //}

                //[Test]
                //public void Verify_projection_service_called_to_load_data_in_database()
                //{
                //    ProjectionService.AssertWasCalled(x => x.Project<Asset, AssetDataDto>(_items));
                //}
            }
            else
            {

                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Verify_handler_executed")
                        .AddTestAttribute(true)
                        .ThrowsNewNotImplementedException(),
                    fullFilename);
            }
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
                .AddUsingStatement(roslynProjectContext.GetNamespaceForDomainModel())
                .AddUsingStatement(roslynProjectContext.GetNamespaceForCoreCqrs(_generationContext.PluralEntityName, _generationContext.ActionName))
                .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
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
                .AddUsingStatement(roslynProjectContext.GetNamespaceForDomainModel())
                .AddUsingStatement(roslynProjectContext.GetNamespaceForCoreCqrs(_generationContext.PluralEntityName, _generationContext.ActionName))
                .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
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
                .AddUsingStatement(roslynProjectContext.GetNamespaceForDomainModel())
                .AddUsingStatement(roslynProjectContext.GetNamespaceForCoreCqrs(_generationContext.PluralEntityName, _generationContext.ActionName))
                .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
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