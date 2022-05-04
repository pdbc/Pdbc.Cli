using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Model;
using Pdbc.Cli.App.Model.Items;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Legacy
{
    public class CodeGenerationService
    {
        private readonly RoslynSolutionContext _roslynSolutionContext;
        private readonly RoslynGenerator _roslynGenerator;

        private GenerationContext _generationContext;

        private readonly StartupParameters _startupParameters2;
        private readonly GenerationConfiguration _generationConfiguration2;
        private readonly FileHelperService _fileHelperService;
        public CodeGenerationService(RoslynSolutionContext roslynSolutionContext,
            StartupParameters startupParameters,
            GenerationConfiguration generationConfiguration)
        {
            _roslynSolutionContext = roslynSolutionContext;
            _startupParameters2 = startupParameters;
            _generationConfiguration2 = generationConfiguration;

            _fileHelperService = new FileHelperService();
            _roslynGenerator = new RoslynGenerator(_fileHelperService);
        }

        
        /// <summary>
        /// This method generates all what is required to setup a new entity in the solution
        /// </summary>
        /// <returns></returns>
        public async Task SetupEntity()
        {
            // TODO Setup GenerationContext based on startup parameters

            _generationContext = new GenerationContext(_startupParameters2, _generationConfiguration2);

            //await GenerateEntity();
            //await GenerateEntityUnitTest();
            //await GenerateEntityTestDataBuilder();
            
            // Repository
            //await GenerateRepositoryInterface();
            //await GenerateRepository();
            //await GenerateRepositoryBaseIntegrationTests();
            //await GenerateRepositoryQueriesIntegrationTests();

            // Entity Framework
            //await GenerateEntityMappingConfiguration();
            //await AppendDatabaseCollectionToDbContext();

            // CQRS
            if (_generationContext.StandardActionInfo.ShouldGenerateCqrs())
            {
                //if (_generationContext.StandardActionInfo.RequiresActionDto())
                //{
                //    await GenerateEntityActionInterfaceDto();
                //    await GenerateEntityActionClassDto();
                //}

                //if (_generationContext.StandardActionInfo.RequiresDataDto())
                //{
                //    await GenerateEntityDataInterfaceDto();
                //    await GenerateEntityDataClassDto();
                //}

                // CQRS
                //await GenerateCqrsInputClass();
                //await GenerateCqrsOutputClass();

                //await GenerateCqrsHandlerClass();
                //await GenerateCqrsHandlerUnitTestClass();

                //await GenerateCqrsValidatorClass();
                //await GenerateCqrsValidatorUnitTestClass();

                //if (_generationContext.StandardActionInfo.RequiresFactory())
                //{
                //    await GenerateCqrsFactoryClass();
                //    await GenerateCqrsFactoryUnitTestClass();
                //}

                //if (_generationContext.StandardActionInfo.RequiresChangesHandler())
                //{
                //    await GenerateCqrsChangesHandlerClass();
                //    await GenerateCqrsChangesHandlerUnitTestClass();
                //}

                //await GenerateRequestInputClass();
                //await GenerateRequestOutputClass();
                //await GenerateServiceContract();

                // add method to controller
                //await GenerateWebApiServiceContractClass();

                //await GenerateCqrsServiceContractInterface();
                //await GenerateCqrsServiceContractClass();
                

                // IntegrationTest
            }
        }





        #region Domain Class + UnitTest skeleton
        //public async Task GenerateEntity()
        //{
        //    var className = _generationContext.EntityName;
        //    var subfolders = new[] { "Model" };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Domain");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        entity = new ClassDeclarationSyntaxBuilder()
        //            .WithName(className)
        //            .ForNamespace(entityNamespace)
        //            .AddUsingStatement("System")
        //            .AddUsingAertssenFrameworkAuditModel()
        //            .AddBaseClass($"BaseEquatableAuditableEntity<{className}>")
        //            .AddBaseClass("IInterfacingEntity")
        //            .Build();

        //        await _fileHelperService.WriteFile(fullFilename, entity);
        //    }

        //    if (!entity.IsPropertyDefined("ExternalSystem"))
        //    {
        //        var propertyDeclaration = new PropertyDeclarationSyntaxBuilder()
        //            .WithName("ExternalSystem")
        //            .ForType("String")
        //            .Build();

        //        entity = entity.AppendMember<ClassDeclarationSyntax>(propertyDeclaration);
        //        await _fileHelperService.WriteFile(fullFilename, entity);
        //    }

        //    // append properties
        //    entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem("String", "ExternalSystem"));
        //    entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem("String", "ExternalIdentification"));
        //    entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem("DateTimeOffset", "DateModified"));

        //    // append methods
        //    entity = await _roslynGenerator.AppendPublicOverridableMethod(fullFilename, entity, new MethodItem("bool", "ShouldAuditPropertyChangeFor"),
        //        new List<PropertyItem>()
        //        {
        //            new PropertyItem("string", "propertyName")
        //        }, "return true;");


        //    entity = await _roslynGenerator.AppendPublicOverridableMethod(fullFilename, entity, new MethodItem("IAuditProperties", "GetAuditProperties"),
        //        new List<PropertyItem>(), @"
        //                return new AuditProperties()
        //                {
        //                    AreaId = this.Id,
        //                    AreaType = """",
        //                    ObjectId = this.Id,
        //                    ObjectType = this.GetType().Name,
        //                    ObjectInfo = $""""
        //               };
        //            ");
        //}

        //public async Task GenerateEntityUnitTest()
        //{
        //    var className = _generationContext.EntityName.ToSpecification();
        //    var subfolders = new[] { "Domain", "Model" };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity != null)
        //    {
        //        return;
        //    }

        //    var entityNamespace = roslynProjectContext.GetNamespace(subfolders);
        //    var usingStatements = new[]
        //    {
        //        "System",
        //        roslynProjectContext.GetNamespaceForDomainModel()
        //    }
        //        .AddAertssenFrameworkAuditModelStatements()
        //        .AddUnitTestUsingStatement();

        //    var baseClasses = new[]
        //    {
        //        $"BaseSpecification"
        //    };

        //    entity = await _roslynGenerator.GenerateTestSpecificationClass(
        //        fullFilename,
        //        entityNamespace,
        //        className,
        //        usingStatements,
        //        baseClasses
        //    );

        //    entity = await _roslynGenerator.AppendAssertionFailTestMethod(fullFilename, entity, new MethodItem("void", "Verify_domain_model_action"));
        //}

        //private async Task GenerateEntityTestDataBuilder()
        //{
        //    var className = _generationContext.EntityName.ToTestDataBuilder();
        //    var subfolders = new[] { "Domain" };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Tests.Helpers");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity != null)
        //    {
        //        return;
        //    }

        //    var entityNamespace = roslynProjectContext.GetNamespace(subfolders);
        //    var usingStatements = new[]
        //    {
        //        "System",
        //        roslynProjectContext.GetNamespaceForDomainModel()
        //    }
        //        .AddUnitTestUsingStatement()
        //        .AddAertssenFrameworkCoreUsingStatements();

        //    var baseClasses = new[]
        //    {
        //        _generationContext.EntityName.ToBuilder(),
        //        $"IExternallyIdentifiableObjectBuilder<{_generationContext.EntityName.ToBuilder()}>"
        //    };

        //    // generate the class
        //    entity = await _roslynGenerator.GeneratePublicClass(
        //        fullFilename,
        //        entityNamespace,
        //        className,
        //        usingStatements,
        //        baseClasses
        //    );

        //}
        #endregion

        #region Repositories
        //private async Task GenerateRepositoryInterface()
        //{
        //    var className = _generationContext.EntityName.ToRepositoryInterface();
        //    var subfolders = new[] { "Repositories" };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);
            
        //    var entity = await roslynProjectContext.GetInterfaceByName(className);
        //    if (entity != null)
        //    {
        //        return;
        //    }

        //    var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //    var usingStatements = new[] { 
        //        "System",
        //        "Aertssen.Framework.Data.Repositories",
        //        roslynProjectContext.GetNamespaceForDomainModel()
        //    }
        //        .AddAertssenFrameworkAuditModelStatements();

        //    var baseClasses = new[]
        //    {
        //        $"IEntityRepository<{_generationContext.EntityName}>"
        //    };

        //    entity = await _roslynGenerator.GeneratePublicInterface(
        //        fullFilename,
        //        entityNamespace,
        //        className,
        //        usingStatements,
        //        baseClasses
        //    );

        //}

        //private async Task GenerateRepository()
        //{
        //    var className = _generationContext.EntityName.ToRepository();
        //    var subfolders = new[] { "Repositories" };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity != null)
        //    {
        //        return;
        //    }

        //    var entityNamespace = roslynProjectContext.GetNamespace(subfolders);
            
        //    var usingStatements = new[] {
        //        "Aertssen.Framework.Data.Repositories",
        //        roslynProjectContext.GetNamespaceForDomainModel()
        //    }
        //        .AddAertssenFrameworkAuditModelStatements();

        //    var baseClasses = new[]
        //    {
        //        $"EntityFrameworkRepository<{_generationContext.EntityName}>",
        //        _generationContext.EntityName.ToRepositoryInterface()
        //    };

        //    entity = await _roslynGenerator.GeneratePublicClass(
        //        fullFilename,
        //        entityNamespace,
        //        className,
        //        usingStatements,
        //        baseClasses
        //    );

        //}

        //public async Task GenerateRepositoryBaseIntegrationTests()
        //{
        //    var className = _generationContext.EntityName.ToRepository()
        //                                                 .ToSpecification();
        //    var subfolders = new[] { "Repositories" };


        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Data");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);
            
        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity != null)
        //    {
        //        return;
        //    }

        //    var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //    var usingStatements = new[] {
        //        "System",
        //        "Aertssen.Framework.Data.Repositories",
        //        roslynProjectContext.GetNamespaceForDomainModelHelpers(),
        //        roslynProjectContext.GetNamespaceForDomainModel()

        //    };
        //    var baseClasses = new[]
        //    {
        //        $"BaseRepositorySpecification<{_generationContext.EntityName}>"
        //    };

        //    entity = await _roslynGenerator.GeneratePublicClass(
        //        fullFilename,
        //        entityNamespace,
        //        className,
        //        usingStatements,
        //        baseClasses
        //    );

        //    entity = await _roslynGenerator.AppendProtectedOverridableMethod(fullFilename, entity,
        //        new MethodItem(_generationContext.EntityName, "CreateExistingItem"),
        //        new List<PropertyItem>(), $"return new {_generationContext.EntityName}TestDataBuilder();");
        //    entity = await _roslynGenerator.AppendProtectedOverridableMethod(fullFilename, entity,
        //        new MethodItem(_generationContext.EntityName, "CreateNewItem"),
        //        new List<PropertyItem>(), $"return new {_generationContext.EntityName}TestDataBuilder();");
        //    entity = await _roslynGenerator.AppendProtectedOverridableMethod(fullFilename, entity,
        //        new MethodItem("void", "EditItem"),
        //        new List<PropertyItem>()
        //        {
        //            new PropertyItem(_generationContext.EntityName, "item")
        //        }, $"throw new NotImplementedException();");
        //}
        //public async Task GenerateRepositoryQueriesIntegrationTests()
        //{
        //    var className = _generationContext.EntityName.ToRepository().ToSpecification("Queries");
        //    var subfolders = new[] { "Extensions" };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Data");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            
        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity != null)
        //    {
        //        return;
        //    }

        //    var entityNamespace = roslynProjectContext.GetNamespace(subfolders);
        //    var usingStatements = new[] {
        //        "System",
        //        "Aertssen.Framework.Data.Repositories",

        //        roslynProjectContext.GetNamespaceForDataRepositories(),
        //        roslynProjectContext.GetNamespaceForIntegationTestDataExtensions(),
        //        roslynProjectContext.GetNamespaceForDomainModelHelpers(),
        //        roslynProjectContext.GetNamespaceForDomainModel()

        //    };
        //    var baseClasses = new[]
        //    {
        //        $"BaseRepositoryExtensionsSpecification<{_generationContext.EntityName.ToRepositoryInterface()}>"
        //    };
        //    entity = await _roslynGenerator.GeneratePublicClass(
        //        fullFilename,
        //        entityNamespace,
        //        className,
        //        usingStatements,
        //        baseClasses
        //    );
        //}
        #endregion

        #region Entity Framework Configuration
        //private async Task GenerateEntityMappingConfiguration()
        //{
        //    var className = _generationContext.EntityName.ToEntityConfigurationClass(); ;
        //    var subfolders = new[] { "Configurations" };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);
            
            
        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity != null)
        //    {
        //        return;
        //    }

        //    var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //    var usingStatements = new[] {
        //        "System",
        //        "Aertssen.Framework.Data.Configurations",
        //        "Microsoft.EntityFrameworkCore",
        //        "Microsoft.EntityFrameworkCore.Metadata.Builders",
        //        roslynProjectContext.GetNamespaceForDomainModel()

        //    };
        //    var baseClasses = new[]
        //    {
        //        $"AuditableIdentifiableMapping<{_generationContext.EntityName}>"
        //    };
        //    entity = await _roslynGenerator.GeneratePublicClass(
        //        fullFilename,
        //        entityNamespace,
        //        className,
        //        usingStatements,
        //        baseClasses
        //    );
            
        //    string bodyStatementToTable = $"builder.ToTable(\"{_generationContext.PluralEntityName}\");";
        //    entity = await _roslynGenerator.AppendPublicOverridableMethod(fullFilename, entity, new MethodItem("void", "Configure"),
        //        new List<PropertyItem>()
        //        {
        //            new PropertyItem($"EntityTypeBuilder <{_generationContext.EntityName}>", "builder")
        //        }, "base.Configure(builder);",
        //        bodyStatementToTable);


        //}
        //private async Task AppendDatabaseCollectionToDbContext()
        //{
        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");
        //    var entity = await roslynProjectContext.GetClassEndingWithName("DbContext");
        //    if (entity == null)
        //    {
        //        throw new InvalidOperationException("Cannot append collection to non-existing (DbContext) class");
        //    }

        //    var fullFilename = entity.SyntaxTree.FilePath;

        //    // TODO Add possibility to add method on specific order in the class !!
        //    entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem($"DbSet<{_generationContext.EntityName}>", _generationContext.PluralEntityName));

        //    //// Get all classes from the project
        //    ////var classes = await _roslynGenerator.LoadClassesAndInterfaces(_context.DataProject) var className = _generationContext.Parameters.EntityConfigurationName;

        //    ////var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");

        //    ////var path = roslynProjectContext.GetPath("Configurations");
        //    ////var filename = $"{className}.cs";
        //    ////var fullFilename = Path.Combine(path, filename);
        //    ////ClassDeclarationSyntax databaseContext = classes.FirstOrDefault(x => x.Identifier.ValueText.EndsWith("DbContext"));
        //    ////if (databaseContext == null)
        //    ////{
        //    ////    throw new InvalidOperationException("Cannot append collection to non-existing (DbContext) class");
        //    ////}

        //    //var propertyName = $"{entityName}Items";

        //    ////databaseContext = databaseContext.AddMembers(propertyDeclarationDbSet);
        //    ////databaseContext.

        //    //var index = 0;
        //    //var counter = 0;
        //    //foreach (var m in databaseContext.Members)
        //    //{
        //    //    counter++;
        //    //    var property = m as PropertyDeclarationSyntax;
        //    //    if (property != null)
        //    //    {
        //    //        var typeSyntax = property.Type.ToFullString();
        //    //        if (property.Identifier.ValueText == propertyName)
        //    //            return;

        //    //        if (typeSyntax.Contains("DbSet"))
        //    //        {
        //    //            index = counter;
        //    //        }
        //    //    }
        //    //}


        //    //var propertyDeclarationDbSet = _roslynGenerator.GenerateProperty($"DbSet<{entityName}>", propertyName);

        //    //var updatedDatabaseContext = databaseContext.WithMembers(databaseContext.Members.Insert(index, propertyDeclarationDbSet));
        //    ////var updatedDatabaseContext = databaseContext.AddMembers(propertyDeclarationDbSet);

        //    //var root = databaseContext.SyntaxTree.GetRoot();
        //    //var filePath = databaseContext.SyntaxTree.FilePath;

        //    //var updatedRoot = root.ReplaceNode(databaseContext, updatedDatabaseContext).NormalizeWhitespace();
        //    //File.WriteAllText(filePath, updatedRoot.ToFullString());


        //}
        #endregion
        
        #region Dto (Data & Action)
        //public async Task GenerateEntityActionInterfaceDto()
        //{
        //    var className = _generationContext.ActionDtoInterface;
        //    var subfolders = new[] { _generationContext.PluralEntityName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Dto");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);
            
        //    // Generate the entity
        //    var entity = await roslynProjectContext.GetInterfaceByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);
        //        var usingStatements = new[] {
        //            "System",
        //        };
        //        var baseClasses = new[]
        //        {
        //            $"IInterfacingDto"
        //        };
        //        entity = await _roslynGenerator.GeneratePublicInterface(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses
        //        );
        //    }
        //}
        //public async Task GenerateEntityActionClassDto()
        //{
        //    var className = _generationContext.ActionDtoClass;
        //    var subfolders = new[] { _generationContext.PluralEntityName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Dto");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);
            
        //    // Generate the entity
        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[] {
        //            "System",
        //        };
        //        var baseClasses = new[]
        //        {
        //            _generationContext.ActionDtoInterface
        //        };
        //        entity = await _roslynGenerator.GeneratePublicClass(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses
        //        );
        //    }

        //    entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem("String", "ExternalSystem"));
        //    entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem("String", "ExternalIdentification"));
        //    entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem("DateTimeOffset", "DateModified"));
        //}

        //public async Task GenerateEntityDataInterfaceDto()
        //{
        //    var className = _generationContext.DataDtoInterface;
        //    var subfolders = new[] { _generationContext.PluralEntityName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Dto");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            

        //    // Generate the entity

        //    var entity = await roslynProjectContext.GetInterfaceByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);
        //        var usingStatements = new[] {
        //            "System",
        //        };
        //        var baseClasses = new List<String>();
        //        entity = await _roslynGenerator.GeneratePublicInterface(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses.ToArray()
        //        );
        //    }
        //}
        //public async Task GenerateEntityDataClassDto()
        //{
        //    var className = _generationContext.DataDtoClass;

        //    var subfolders = new[] { _generationContext.PluralEntityName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Dto");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[] {
        //            "System",
        //        };
        //        var baseClasses = new[]
        //        {
        //            _generationContext.DataDtoInterface
        //        };
        //        entity = await _roslynGenerator.GeneratePublicClass(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses
        //        );
        //    }
        //}
        #endregion

        #region Requests
        //public async Task GenerateRequestInputClass()
        //{
        //    var className = _generationContext.RequestInputClassName;
        //    var subfolders = new[] { "Requests", _generationContext.PluralEntityName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Api.Contracts");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    // Generate the entity

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[]
        //        {
        //            "System",
        //            roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName)
        //        }
        //            .AddAertssenFrameworkContractUsingStatements();

        //        // TODO Get the correct base class 
        //        // List => AertssenListRequest
        //        var baseClasses = new[]
        //        {
        //            "AertssenRequest"
        //        };

        //        entity = await _roslynGenerator.GeneratePublicClass(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses
        //        );
        //    }

        //    if (_generationContext.StandardActionInfo.RequiresActionDto())
        //    {
        //        entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem(_generationContext.ActionDtoClass, _generationContext.EntityName));
        //    }
        //}

        //public async Task GenerateRequestOutputClass()
        //{
        //    var className = _generationContext.CqrsOutputClassName;

        //    var subfolders = new[] { "Requests", _generationContext.PluralEntityName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Api.Contracts");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);


        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[] {
        //            "System",
        //            roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName)
        //        }
        //            .AddAertssenFrameworkContractUsingStatements();
        //        // TODO Get the correct base class 
        //        // List => AertssenListResponse<AssetDataDto> (oData)
        //        var baseClasses = new[]
        //        {
        //            "AertssenResponse"
        //        };
        //        entity = await _roslynGenerator.GeneratePublicClass(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses.ToArray()
        //        );
        //    }

        //    if (_generationContext.StandardActionInfo.RequiresDataDto())
        //    {
        //        entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem(_generationContext.DataDtoClass, _generationContext.EntityName));
        //    }
        //}

        #endregion

        #region Service Contract

        //public async Task GenerateServiceContract()
        //{
        //    var className = _generationContext.ServiceContractInterfaceName;
        //    var subfolders = new[] { "Services", _generationContext.PluralEntityName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Api.Contracts");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    // Generate the entity

        //    var entity = await roslynProjectContext.GetInterfaceByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[]
        //        {
        //            "System",
        //            roslynProjectContext.GetNamespaceForRequests(_generationContext.PluralEntityName),
        //            roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName)
        //        }
        //            .AddAertssenFrameworkContractUsingStatements();

        //        // TODO Get the correct base class 
        //        // List => AertssenListRequest
        //        var baseClasses = new List<String>();

        //        entity = await _roslynGenerator.GeneratePublicInterface(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses.ToArray()
        //        );
        //    }


        //    //Task<ListAssetsResponse> ListAssets(ListAssetsRequest request);
        //    //entity = await _roslynGenerator.AppendServiceContractMethod(fullFilename, entity, _generationContext);
        //}


        #endregion

        #region Cqrs
        //public async Task GenerateCqrsInputClass()
        //{
        //    var className = _generationContext.CqrsInputClassName;
        //    var subfolders = new[]
        //        {"CQRS", _generationContext.PluralEntityName, _generationContext.ActionName};

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    // Generate the entity

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[]
        //        {
        //            "System",
        //            "Aertssen.Framework.Infra.CQRS.Base",
        //            roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName)
        //        };
        //        var baseClasses = new[]
        //        {
        //            $"I{_generationContext.CqrsInputType}<{_generationContext.CqrsOutputClassName}>"
        //        };
        //        entity = await _roslynGenerator.GeneratePublicClass(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses
        //        );
        //    }

        //    if (_generationContext.StandardActionInfo.RequiresActionDto())
        //    {
        //        entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem(_generationContext.ActionDtoInterface, _generationContext.EntityName));
        //    }
        //}

        //public async Task GenerateCqrsOutputClass()
        //{
        //    var className = _generationContext.CqrsOutputClassName;

        //    var subfolders = new[] { "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName};

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);
            
            
        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[] {
        //            "System",
        //            roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName)
        //        };
        //        var baseClasses = new List<String>();
        //        entity = await _roslynGenerator.GeneratePublicClass(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses.ToArray()
        //        );
        //    }

        //    if (_generationContext.StandardActionInfo.RequiresDataDto())
        //    {
        //        entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem(_generationContext.DataDtoInterface, _generationContext.EntityName));
        //    }
        //}
        
        //public async Task GenerateCqrsHandlerClass()
        //{
        //    var className = _generationContext.CqrsHandlerClassName;

        //    var subfolders = new[] { "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[] {
        //            "System",
        //            "System.Threading",
        //            "System.Threading.Tasks",
        //            "Aertssen.Framework.Infra.CQRS.Base",
        //            "AutoMapper",
        //            roslynProjectContext.GetNamespaceForDomainModel(),
        //            roslynProjectContext.GetNamespaceForData(),
        //            roslynProjectContext.GetNamespaceForDataRepositories(),
        //            roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName),
        //            "MediatR"
        //    };
        //        var baseClasses = new[]
        //        {
        //            $"IRequestHandler<{_generationContext.CqrsInputClassName}, { _generationContext.CqrsOutputClassName}>"
        //        };
        //        entity = await _roslynGenerator.GeneratePublicClass(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses
        //        );
        //    }

        //    // Variable declarations
        //    entity = await _roslynGenerator.AppendClassVariable(fullFilename, entity,
        //        new PropertyItem($"IFactory<{_generationContext.ActionDtoInterface}, {_generationContext.EntityName}>", "_factory"));
        //    entity = await _roslynGenerator.AppendClassVariable(fullFilename, entity,
        //        new PropertyItem($"IChangesHandler<{_generationContext.ActionDtoInterface}, {_generationContext.EntityName}>", "_changesHandler"));
        //    entity = await _roslynGenerator.AppendClassVariable(fullFilename, entity,
        //        new PropertyItem($"{_generationContext.EntityName.ToRepositoryInterface()}", "_repository"));


        //    // Constructor
        //    //public Task<AddressStoreResult> Handle(AddressStoreCommand request, CancellationToken cancellationToken)
        //    //{
        //    //    throw new NotImplementedException();
        //    //}

        //    // Handle Method
        //    entity = await _roslynGenerator.AppendPublicMethodNotImplemented(fullFilename, entity, new MethodItem($"Task<{_generationContext.CqrsOutputType}>", "Handle"),
        //        new List<PropertyItem>()
        //        {
        //            new PropertyItem(_generationContext.CqrsInputClassName, "request"),
        //            new PropertyItem("CancellationToken", "cancellationToken")
        //        });

        //    //entity = await _roslynGenerator.AppendCqrsHandlerStoreMethod(fullFilename, entity, _generationContext.StandardActionInfo);
        //}

        //public async Task GenerateCqrsValidatorClass()
        //{
        //    var className = _generationContext.CqrsValidatorClassName;

        //    var subfolders = new[] { "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[] {
        //            "Aertssen.Framework.Infra.Validation",
        //            "FluentValidation",
        //            roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName)
        //        };
        //        var baseClasses = new[]
        //        {
        //            $"FluentValidationValidator<{_generationContext.CqrsInputClassName}>"
        //        };
        //        entity = await _roslynGenerator.GeneratePublicClass(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses
        //        );
        //    }
        //}

        //public async Task GenerateCqrsFactoryClass()
        //{
        //    var className = _generationContext.CqrsFactoryClassName;

        //    var subfolders = new[] { "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[] {
        //            "System",
        //            "Aertssen.Framework.Infra.CQRS.Base",
        //            roslynProjectContext.GetNamespaceForDomainModel(),
        //            roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName)
        //        };
        //        var baseClasses = new[]
        //        {
        //            $"IFactory<{_generationContext.ActionDtoInterface},{_generationContext.EntityName}>"
        //        };
        //        entity = await _roslynGenerator.GeneratePublicClass(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses
        //        );
        //    }

        //    // append methods
        //    entity = await _roslynGenerator.AppendPublicMethodNotImplemented(fullFilename, entity, new MethodItem(_generationContext.EntityName, "Create"),
        //        new List<PropertyItem>()
        //        {
        //            new PropertyItem(_generationContext.ActionDtoClass.ToInterface(), "model")
        //        });
        //}

        //public async Task GenerateCqrsChangesHandlerClass()
        //{
        //    var className = _generationContext.CqrsChangesHandlerClassName;

        //    var subfolders = new[] { "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[] {
        //            "System",
        //            "Aertssen.Framework.Infra.Validation",
        //            "Aertssen.Framework.Infra.CQRS.Base",
        //            "FluentValidation",
        //            roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName),
        //            roslynProjectContext.GetNamespaceForDomainModel()
        //        };
        //        var baseClasses = new[]
        //        {
        //            $"IChangesHandler<{_generationContext.ActionDtoInterface},{_generationContext.EntityName}>"
        //        };
        //        entity = await _roslynGenerator.GeneratePublicClass(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses
        //        );
        //    }

        //    entity = await _roslynGenerator.AppendPublicMethodNotImplemented(fullFilename, entity, new MethodItem("void", "ApplyChanges"),
        //        new List<PropertyItem>()
        //        {
        //            new PropertyItem(_generationContext.EntityName, "entity"),
        //            new PropertyItem(_generationContext.ActionDtoClass.ToInterface(), "model")
        //        });
        //}
        #endregion
        
        #region Cqrs - UnitTests

        //public async Task GenerateCqrsHandlerUnitTestClass()
        //{
        //    var className = _generationContext.CqrsHandlerClassName.ToSpecification();

        //    var subfolders = new[] { "Core", "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);
            
        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity != null)
        //    {
        //        return;
        //    }

        //    var entityNamespace = roslynProjectContext.GetNamespace(subfolders);
        //    var usingStatements = new[]
        //    {
        //        "System",
        //        "System.Collections.Generic",
        //        "Aertssen.Framework.Tests",
        //        "Aertssen.Framework.Tests.Extensions",
        //        "NUnit.Framework",
        //        "Aertssen.Framework.Audit.Core.Model.Base",
        //        roslynProjectContext.GetNamespaceForCoreCqrs(_generationContext.PluralEntityName, _generationContext.ActionName),
        //        roslynProjectContext.GetNamespaceForDomainModel() };
            
        //    var baseClasses = new[] { $"{_generationContext.CqrsHandlerClassName.ToContextSpecification()}>" };

        //    entity = await _roslynGenerator.GenerateTestSpecificationClass(
        //        fullFilename,
        //        entityNamespace,
        //        className,
        //        usingStatements,
        //        baseClasses
        //    );

        //    entity = await _roslynGenerator.AppendAssertionFailTestMethod(fullFilename, entity, new MethodItem("void", "Verify_handler_executed"));
        //}
        
        //public async Task GenerateCqrsValidatorUnitTestClass()
        //{
        //    var className = _generationContext.CqrsValidatorClassName.ToSpecification();
            
        //    var subfolders = new[] { "Core", "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity != null)
        //    {
        //        return;
        //    }

        //    var entityNamespace = roslynProjectContext.GetNamespace(subfolders);
        //    var usingStatements = new[]
        //    {
        //        "System",
        //        "System.Collections.Generic",
        //        "Aertssen.Framework.Tests",
        //        "Aertssen.Framework.Tests.Extensions",
        //        "NUnit.Framework",
        //        "Aertssen.Framework.Audit.Core.Model.Base",
        //        roslynProjectContext.GetNamespaceForCoreCqrs(_generationContext.PluralEntityName, _generationContext.ActionName),
        //        roslynProjectContext.GetNamespaceForDomainModel()
        //    };
        //    var baseClasses = new[] { $"{_generationContext.CqrsValidatorClassName.ToContextSpecification()}" };

        //    entity = await _roslynGenerator.GenerateTestSpecificationClass(
        //        fullFilename,
        //        entityNamespace,
        //        className,
        //        usingStatements,
        //        baseClasses
        //    );

        //    entity = await _roslynGenerator.AppendAssertionFailTestMethod(fullFilename, entity, new MethodItem("void", "Verify_validator_logic_executed"));
        //}


        //public async Task GenerateCqrsFactoryUnitTestClass()
        //{
        //    var className = _generationContext.CqrsFactoryClassName.ToSpecification();

        //    var subfolders = new[] { "Core", "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity != null)
        //    {
        //        return;
        //    }

        //    var entityNamespace = roslynProjectContext.GetNamespace(subfolders);
        //    var usingStatements = new[]
        //    {
        //        "System",
        //        "System.Collections.Generic",
        //        "Aertssen.Framework.Tests",
        //        "Aertssen.Framework.Tests.Extensions",
        //        "NUnit.Framework",
        //        "Aertssen.Framework.Audit.Core.Model.Base",
        //        roslynProjectContext.GetNamespaceForCoreCqrs(_generationContext.PluralEntityName, _generationContext.ActionName),
        //        roslynProjectContext.GetNamespaceForDomainModel() };
        //    var baseClasses = new[] { $"{_generationContext.CqrsFactoryClassName.ToContextSpecification()}" };

        //    entity = await _roslynGenerator.GenerateTestSpecificationClass(
        //        fullFilename,
        //        entityNamespace,
        //        className,
        //        usingStatements,
        //        baseClasses
        //    );

        //    entity = await _roslynGenerator.AppendAssertionFailTestMethod(fullFilename, entity, new MethodItem("void", "Verify_factory_logic_executed"));
        //}


        //public async Task GenerateCqrsChangesHandlerUnitTestClass()
        //{
        //    var className = _generationContext.CqrsChangesHandlerClassName.ToSpecification();

        //    var subfolders = new[] { "Core", "CQRS", _generationContext.PluralEntityName, _generationContext.ActionName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity != null)
        //    {
        //        return;
        //    }

        //    var entityNamespace = roslynProjectContext.GetNamespace(subfolders);
        //    var usingStatements = new[]
        //    {
        //        "System",
        //        "System.Collections.Generic",
        //        "Aertssen.Framework.Tests",
        //        "Aertssen.Framework.Tests.Extensions",
        //        "NUnit.Framework",
        //        "Aertssen.Framework.Audit.Core.Model.Base",
        //        roslynProjectContext.GetNamespaceForCoreCqrs(_generationContext.PluralEntityName, _generationContext.ActionName),
        //        roslynProjectContext.GetNamespaceForDomainModel() };
        //    var baseClasses = new[] { $"{_generationContext.CqrsChangesHandlerClassName.ToContextSpecification()}" };

        //    entity = await _roslynGenerator.GenerateTestSpecificationClass(
        //        fullFilename,
        //        entityNamespace,
        //        className,
        //        usingStatements,
        //        baseClasses
        //    );

        //    entity = await _roslynGenerator.AppendAssertionFailTestMethod(fullFilename, entity, new MethodItem("void", "Verify_changes_handler_logic_executed"));
        //}

        #endregion

        #region Service Contract Implementations
        
        //public async Task GenerateCqrsServiceContractInterface()
        //{
        //    var className = _generationContext.CqrsServiceContractInterfaceName;
        //    var subfolders = new[] { "Services", _generationContext.PluralEntityName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Services.Cqrs");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    // Generate the entity

        //    var entity = await roslynProjectContext.GetInterfaceByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[]
        //        {
        //            "System",
        //            //"Locations.Api.Contracts.Services.Addresses",
        //            roslynProjectContext.GetNamespaceForRequests(_generationContext.PluralEntityName),
        //            roslynProjectContext.GetNamespaceForServices(_generationContext.PluralEntityName),
        //            roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName)
        //        }
        //            .AddAertssenFrameworkContractUsingStatements();

        //        var baseClasses = new string[]
        //        {
        //            _generationContext.ServiceContractInterfaceName
        //        };

        //        entity = await _roslynGenerator.GeneratePublicInterface(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses.ToArray()
        //        );
        //    }


        //    //Task<ListAssetsResponse> ListAssets(ListAssetsRequest request);
        //    //entity = await _roslynGenerator.AppendServiceContractMethod(fullFilename, entity, _generationContext);
        //}

        //public async Task GenerateCqrsServiceContractClass()
        //{
        //    var className = _generationContext.CqrsServiceContractName;
        //    var subfolders = new[] { "Services", _generationContext.PluralEntityName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Services.Cqrs");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    // Generate the entity

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[]
        //        {
        //            "System",
        //            "Aertssen.Framework.Services.Cqrs.Base",
        //            "MediatR",
        //            "AutoMapper",
        //            "Aertssen.Framework.Core.Validation",
        //            roslynProjectContext.GetNamespaceForRequests(_generationContext.PluralEntityName),
        //            roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName)
        //        }
        //            .AddAertssenFrameworkContractUsingStatements();

        //        var baseClasses = new string[]
        //        {
        //            "CqrsService",
        //            _generationContext.CqrsServiceContractName.ToInterface()
        //        };

        //        entity = await _roslynGenerator.GeneratePublicClass(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses.ToArray()
        //        );
        //    }

        //    //public AddressesCqrsService(IMediator mediator,
        //    //    IMapper mapper,
        //    //    IValidationBag validationBag) : base(mediator, mapper, validationBag)
        //    //{
        //    //}
        //    var parameters = new List<PropertyItem>()
        //    {
        //        new PropertyItem("IMediator", "mediator"),
        //        new PropertyItem("IMapper", "mapper"),
        //        new PropertyItem("IValidationBag", "validationBag"),
        //    };
        //    await _roslynGenerator.GenerateConstructor(fullFilename, _generationContext.CqrsServiceContractName, entity, parameters);
        //    //Task<ListAssetsResponse> ListAssets(ListAssetsRequest request);
        //    //entity = await _roslynGenerator.AppendServiceContractMethod(fullFilename, entity, _generationContext);
        //}

        //public async Task GenerateWebApiServiceContractClass()
        //{
        //    var className = _generationContext.WebApiServiceContractName;
        //    var subfolders = new[] { _generationContext.PluralEntityName };

        //    var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Api.ServiceAgent");
        //    var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

        //    // Generate the entity

        //    var entity = await roslynProjectContext.GetClassByName(className);
        //    if (entity == null)
        //    {
        //        var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

        //        var usingStatements = new[]
        //        {
        //            "System",
        //            "Aertssen.Framework.Api.ServiceAgents",
        //            "Microsoft.Extensions.Logging",
        //            roslynProjectContext.GetNamespaceForServices(_generationContext.PluralEntityName),
        //            roslynProjectContext.GetNamespaceForRequests(_generationContext.PluralEntityName),
        //            roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName)
        //        }
        //            .AddAertssenFrameworkContractUsingStatements();

        //        var baseClasses = new string[]
        //        {
        //            "WebApiService",
        //            _generationContext.ServiceContractInterfaceName
        //        };

        //        entity = await _roslynGenerator.GeneratePublicClass(
        //            fullFilename,
        //            entityNamespace,
        //            className,
        //            usingStatements,
        //            baseClasses.ToArray()
        //        );
        //    }

        //    var parameters = new List<PropertyItem>()
        //    {
        //        new PropertyItem("Func<IWebApiClientProxy>", "webApiClientFactory"),
        //        new PropertyItem($"ILogger<{_generationContext.WebApiServiceContractName}>", "logger")
        //    };
        //    await _roslynGenerator.GenerateConstructor(fullFilename, _generationContext.WebApiServiceContractName, entity, parameters);
        //    //Task<ListAssetsResponse> ListAssets(ListAssetsRequest request);
        //    //entity = await _roslynGenerator.AppendServiceContractMethod(fullFilename, entity, _generationContext);
        //}

        #endregion


        //private async Task SetupEntityActionDto(string entityName, string action, string classType)
        //{

        //    //var className = $"{action}{entityName}Dto";
        //    //var interfaceClassName = $"I{className}";

        //    //// Get all classes from the project
        //    //var classes = await _roslynGenerator.LoadClassesAndInterfaces(_context.DtoProject);
        //    //var entity = classes.FirstOrDefault(x => x.Identifier.ValueText == className);
        //    //if (entity != null)
        //    //{
        //    //    // Entity already exist in the project, no need to generate it
        //    //    return
        //    //}

        //    //var entityNamespace = _context.GetCoreNamespace($"{entityName}.{action}");

        //    //// Generation Code
        //    //var @namespace = _roslynGenerator.CreateNamespaceDeclaration(entityNamespace)
        //    //    .AddUsingStatements("System", "Aertssen.Framework.Infra.CQRS.Base", _context.GetDtoNamespace(entityName));

        //    ////  Create a class
        //    //var classDeclaration = _roslynGenerator.CreateClassDeclaration(className)
        //    //    .AsPublic()
        //    //    .AddBaseClasses($"ICommand<{dtoResultClassName}>");


        //    //// Generate Properties
        //    //var propertyDeclarationExternalSystem = _roslynGenerator.GenerateProperty(dtoInterfaceClassName, entityName);

        //    //// Add properties/methods to class Declaration
        //    //classDeclaration = classDeclaration.AddMembers(
        //    //    propertyDeclarationExternalSystem);

        //    //// Add the class to the namespace.
        //    //WriteFile(_context.GetCorePath("CQRS", entityName, action), className, @namespace, classDeclaration);
        //}


        //private async Task SetupCommandQueryClass(string entityName, string action, string classType)
        //{
        //    var className = $"{action}{entityName}{classType}"; // Command vs Query
        //    var dtoResultClassName = $"{action}{entityName}Result";   // ViewModel vs Result
        //    var dtoInterfaceClassName = $"I{action}{entityName}Dto";   

        //    // Get all classes from the project
        //    var classes = await _roslynGenerator.LoadClassesAndInterfaces(_context.CoreProject);
        //    var entity = classes.FirstOrDefault(x => x.Identifier.ValueText == className);
        //    if (entity != null)
        //    {
        //        // Entity already exist in the project, no need to generate it
        //        return;
        //    }

        //    var entityNamespace = _context.GetCoreNamespace($"{entityName}.{action}");

        //    // Generation Code
        //    var @namespace = _roslynGenerator.CreateNamespaceDeclaration(entityNamespace)
        //        .AddUsingStatements("System", "Aertssen.Framework.Infra.CQRS.Base", _context.GetDtoNamespace(entityName));

        //    //  Create a class
        //    var classDeclaration = _roslynGenerator.CreateClassDeclaration(className)
        //        .AsPublic()
        //        .AddBaseClasses($"ICommand<{dtoResultClassName}>");


        //    // Generate Properties
        //    var propertyDeclarationExternalSystem = _roslynGenerator.GenerateProperty(dtoInterfaceClassName, entityName);

        //    // Add properties/methods to class Declaration
        //    classDeclaration = classDeclaration.AddMembers(
        //        propertyDeclarationExternalSystem);

        //    // Add the class to the namespace.
        //    WriteFile(_context.GetCorePath("CQRS", entityName,action), className, @namespace, classDeclaration);
        //}















        ////private async Task<Document> AddProperty(Document document, ClassDeclarationSyntax classeDecl, CancellationToken cancellationToken)
        ////{
        ////    var root = await document.GetSyntaxRootAsync(cancellationToken);
        ////    var newClass = classeDecl.AddMembers(MakeProperty());
        ////    return document.WithSyntaxRoot(root.ReplaceNode(classeDecl, newClass));
        ////}

        //private void WriteFile(string path, String entityName, NamespaceDeclarationSyntax @namespace, params MemberDeclarationSyntax[] members)
        //{
        //    //@namespace = @namespace.AddMembers(members);

        //    var code = GenerateCodenerateCode(@namespace, members);
        //    _fileHelperService.WriteFile(path, entityName, code);
        //}

        //private static string GenerateCodenerateCode(NamespaceDeclarationSyntax @namespace, params MemberDeclarationSyntax[] members)
        //{
        //    @namespace = @namespace.AddMembers(members);

        //    // Normalize and get code as string.
        //    var code = @namespace
        //        .NormalizeWhitespace()
        //        .ToFullString();
        //    return code;
        //}

    }
}