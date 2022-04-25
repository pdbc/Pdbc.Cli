using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Model;
using Pdbc.Cli.App.Roslyn;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Visitors;

namespace Pdbc.Cli.App
{
    public class CodeGenerationService
    {
        private readonly RoslynSolutionContext _roslynSolutionContext;
        private readonly GenerationContext _generationContext;
        private readonly RoslynGenerator _roslynGenerator;

        private readonly FileHelperService _fileHelperService;

        public CodeGenerationService(RoslynSolutionContext roslynSolutionContext, GenerationContext generationContext)
        {
            _roslynSolutionContext = roslynSolutionContext;
            _generationContext = generationContext;
            _fileHelperService = new FileHelperService();

            _roslynGenerator = new RoslynGenerator(_fileHelperService);
        }

        /// <summary>
        /// This method generates all what is required to setup a new entity in the solution
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public async Task SetupEntity()
        {
            await GenerateEntity();
            await GenerateEntityUnitTest();
            await GenerateEntityTestDataBuilder();


            await GenerateRepositoryInterface();
            await GenerateRepository();
            await GenerateRepositoryBaseIntegrationTests();
            await GenerateRepositoryQueriesIntegrationTests();

            await GenerateEntityMappingConfiguration();
            await AppendDatabaseCollectionToDbContext();

            if (_generationContext.Parameters.ShouldGenerateCqrs())
            {
                await GenerateEntityActionInterfaceDto();
                await GenerateEntityActionClassDto();

                await GenerateCqrsInputClass();
                //await GenerateCqrsOutputClass();

            }
        }

        public async Task SetupEntityAction()
        {
            await SetupEntity();

            // add method to controller
            // add method to IServiceContract
            // add method to implementation WebApiServiceContract
            // add method to implementation CqrsServiceContract

            // add dto 
            //await SetupEntityActionDto(entityName, action, "Command");

            // add command/query in cqrs
            // add command/query handler
            // add command/query handler unittests
            // add command/query validator
            // add command/query unittests




            //await SetupCommandQueryClass(entityName, action, "Command");

        }

        #region Domain Class + UnitTest skeleton
        public async Task GenerateEntity()
        {
            var className = _generationContext.Parameters.EntityName;

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Domain");

            var path = roslynProjectContext.GetPath("Model");
            var filename = $"{className}.cs";
            var fullFilename = Path.Combine(path, filename);
            
            // Generate the entity
           
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace("Model");
                var usingStatements = new[] {"System", "Aertssen.Framework.Audit.Core.Model.Base"};
                var baseClasses = new[] {$"BaseEquatableAuditableEntity<{className}>", "IInterfacingEntity"};


                //var @namespace = await _roslynGenerator.GenerateNamespace(entityNamespace, new[] { "System", "Aertssen.Framework.Audit.Core.Model.Base" });

                // generate the class
                entity = await _roslynGenerator.GeneratePublicClass(
                    fullFilename,
                    entityNamespace,
                    className,
                    usingStatements,
                    baseClasses
                );
                
            }

            // append properties
            entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem( "String", "ExternalSystem"));
            entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem("String", "ExternalIdentification"));

            entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem("DateTimeOffset", "DateModified"));
            entity = await _roslynGenerator.AppendPublicOverridableMethod(fullFilename, entity, new MethodItem("bool", "ShouldAuditPropertyChangeFor"),
                new List<PropertyItem>()
                {
                    new PropertyItem("string", "propertyName")
                }, "return true;");

            entity = await _roslynGenerator.AppendPublicOverridableMethod(fullFilename, entity, new MethodItem("IAuditProperties", "GetAuditProperties"),
                new List<PropertyItem>(), @"
                        return new AuditProperties()
                        {
                            AreaId = this.Id,
                            AreaType = """",
                            ObjectId = this.Id,
                            ObjectType = this.GetType().Name,
                            ObjectInfo = $""""
                       };
                    ");
        }

        public async Task GenerateEntityUnitTest()
        {
            var className = _generationContext.Parameters.EntitySpecificationName;

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("UnitTests");

            var path = roslynProjectContext.GetTestsPath("Domain","Model");
            var filename = $"{className}.cs";
            var fullFilename = Path.Combine(path, filename);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }


            var entityNamespace = roslynProjectContext.GetNamespace("Domain.Model");
            var usingStatements = new[] { "System","Aertssen.Framework.Tests",
                "Aertssen.Framework.Tests.Extensions",
                "NUnit.Framework",
                "Aertssen.Framework.Audit.Core.Model.Base",
                roslynProjectContext.GetNamespaceForDomainModel() };
            var baseClasses = new[] { $"BaseSpecification" };

            entity = await _roslynGenerator.GenerateTestSpecificationClass(
                fullFilename,
                entityNamespace,
                className,
                usingStatements,
                baseClasses
            );
            //var @namespace = await _roslynGenerator.GenerateNamespace(entityNamespace, new[] { "System", 
            //    "Aertssen.Framework.Tests",
            //    "Aertssen.Framework.Tests.Extensions",
            //    "NUnit.Framework",
            //    "Aertssen.Framework.Audit.Core.Model.Base",
            //    roslynProjectContext.GetNamespaceForDomainModel()
            //});

            //// generate the class
            //entity = await _roslynGenerator.GenerateTestSpecificationClass(
            //    fullFilename,
            //    @namespace,
            //    className,
            //    new[] { $"BaseSpecification" }
            //);

            entity = await _roslynGenerator.AppendAssertionFailTestMethod(fullFilename, entity, new MethodItem("void", "Verify_domain_model_action"));
        }

        private async Task GenerateEntityTestDataBuilder()
        {
            var className = _generationContext.Parameters.EntityTestDataBuilderName;

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Tests.Helpers");

            var path = roslynProjectContext.GetTestsPath("Domain");
            var filename = $"{className}.cs";
            var fullFilename = Path.Combine(path, filename);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace("Domain");
            var usingStatements = new[]
            {
                "System","Aertssen.Framework.Tests",
                "Aertssen.Framework.Core.Builders",
                "Aertssen.Framework.Core.Extensions",
                "Aertssen.Framework.Tests",
                roslynProjectContext.GetNamespaceForDomainModel()
            };
            var baseClasses = new[] { $"{_generationContext.Parameters.EntityBuilderName}", $"IExternallyIdentifiableObjectBuilder<{_generationContext.Parameters.EntityBuilderName}>" };

            // generate the class
            entity = await _roslynGenerator.GeneratePublicClass(
                fullFilename,
                entityNamespace,
                className,
                usingStatements,
                baseClasses
            );

            //var @namespace = await _roslynGenerator.GenerateNamespace(entityNamespace, new[] { "System",
            //    "Aertssen.Framework.Core.Builders",
            //    "Aertssen.Framework.Core.Extensions",
            //    "Aertssen.Framework.Tests",
            //    roslynProjectContext.GetNamespaceForDomainModel()
            //});

            //// generate the class
            //entity = await _roslynGenerator.GeneratePublicClass(
            //    fullFilename,
            //    @namespace,
            //    className,
            //    new[] { $"{_generationContext.Parameters.EntityBuilderName}", $"IExternallyIdentifiableObjectBuilder<{_generationContext.Parameters.EntityBuilderName}>" }
            //);
        }
        #endregion

        #region Repositories
        private async Task GenerateRepositoryInterface()
        {
            var className = _generationContext.Parameters.EntityRepositoryInterfaceName;

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");

            var path = roslynProjectContext.GetPath("Repositories");
            var filename = $"{className}.cs";
            var fullFilename = Path.Combine(path, filename);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace("Repositories");
            
            var usingStatements = new[] { "System","Aertssen.Framework.Data.Repositories",
                "Aertssen.Framework.Audit.Core.Model.Base",
                roslynProjectContext.GetNamespaceForDomainModel()

            };
            var baseClasses = new[]
            {
                $"IEntityRepository<{_generationContext.Parameters.EntityName}>"
            };
            entity = await _roslynGenerator.GeneratePublicInterface(
                fullFilename,
                entityNamespace,
                className,
                usingStatements,
                baseClasses
            );

            //var @namespace = await _roslynGenerator.GenerateNamespace(entityNamespace, new[] { "System",
            //    "Aertssen.Framework.Data.Repositories",
            //    "Aertssen.Framework.Audit.Core.Model.Base",
            //    roslynProjectContext.GetNamespaceForDomainModel()
            //});

            // generate the class


        }

        private async Task GenerateRepository()
        {
            var className = _generationContext.Parameters.EntityRepositoryClassName;

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");

            var path = roslynProjectContext.GetPath("Repositories");
            var filename = $"{className}.cs";
            var fullFilename = Path.Combine(path, filename);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace("Repositories");


            var usingStatements = new[] {
                "Aertssen.Framework.Data.Repositories",
                "Aertssen.Framework.Audit.Core.Model.Base",
                roslynProjectContext.GetNamespaceForDomainModel()

            };
            var baseClasses = new[]
            {
                $"EntityFrameworkRepository<{_generationContext.Parameters.EntityName}>", 
                _generationContext.Parameters.EntityRepositoryInterfaceName
            };
            entity = await _roslynGenerator.GeneratePublicClass(
                fullFilename,
                entityNamespace,
                className,
                usingStatements,
                baseClasses
            );


            //var @namespace = await _roslynGenerator.GenerateNamespace(entityNamespace, new[] { "System",
            //    "Aertssen.Framework.Data.Repositories",
            //    "Aertssen.Framework.Audit.Core.Model.Base",
            //    roslynProjectContext.GetNamespaceForDomainModel()
            //});

            //// generate the class
            //entity = await _roslynGenerator.GeneratePublicClass(
            //    fullFilename,
            //    @namespace,
            //    className,
            //    new[] { $"EntityFrameworkRepository<{_generationContext.Parameters.EntityName}>", _generationContext.Parameters.EntityRepositoryInterfaceName }
            //);
        }

        public async Task GenerateRepositoryBaseIntegrationTests()
        {
            var className = _generationContext.Parameters.EntityRepositorySpecificationName;

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Data");

            var path = roslynProjectContext.GetTestsPath("Base");
            var filename = $"{className}.cs";
            var fullFilename = Path.Combine(path, filename);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace();

            var usingStatements = new[] {
                "System",
                "Aertssen.Framework.Data.Repositories",
                roslynProjectContext.GetNamespaceForDomainModelHelpers(),
                roslynProjectContext.GetNamespaceForDomainModel()

            };
            var baseClasses = new[]
            {
                $"BaseRepositorySpecification<{_generationContext.Parameters.EntityName}>"
            };
            entity = await _roslynGenerator.GeneratePublicClass(
                fullFilename,
                entityNamespace,
                className,
                usingStatements,
                baseClasses
            );


            //var @namespace = await _roslynGenerator.GenerateNamespace(entityNamespace, new[]
            //{
            //    "System",
            //    "Aertssen.Framework.Data.Repositories",
            //    roslynProjectContext.GetNamespaceForDomainModelHelpers(),
            //    roslynProjectContext.GetNamespaceForDomainModel()
            //});

            //// generate the class
            //entity = await _roslynGenerator.GeneratePublicClass(
            //    fullFilename,
            //    @namespace,
            //    className,
            //    new[]
            //    {
            //        $"BaseRepositorySpecification<{_generationContext.Parameters.EntityName}>"
            //    }
            //);

            entity = await _roslynGenerator.AppendProtectedOverridableMethod(fullFilename, entity,
                new MethodItem(_generationContext.Parameters.EntityName, "CreateExistingItem"),
                new List<PropertyItem>(), $"return new {_generationContext.Parameters.EntityName}TestDataBuilder();");
            entity = await _roslynGenerator.AppendProtectedOverridableMethod(fullFilename, entity,
                new MethodItem(_generationContext.Parameters.EntityName, "CreateNewItem"),
                new List<PropertyItem>(), $"return new {_generationContext.Parameters.EntityName}TestDataBuilder();");
            entity = await _roslynGenerator.AppendProtectedOverridableMethod(fullFilename, entity,
                new MethodItem("void", "EditItem"),
                new List<PropertyItem>()
                {
                    new PropertyItem(_generationContext.Parameters.EntityName, "item")
                }, $"throw new NotImplementedException();");
        }

        public async Task GenerateRepositoryQueriesIntegrationTests()
        {


            var className = _generationContext.Parameters.EntityRepositorySpecificationQueriesName;

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Data");

            var path = roslynProjectContext.GetTestsPath("Extensions");
            var filename = $"{className}.cs";
            var fullFilename = Path.Combine(path, filename);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace("Data.Extensions");
            var usingStatements = new[] {
                "System",
                "Aertssen.Framework.Data.Repositories",

                roslynProjectContext.GetNamespaceForDataRepositories(),
                roslynProjectContext.GetNamespaceForIntegationTestDataExtensions(),
                roslynProjectContext.GetNamespaceForDomainModelHelpers(),
                roslynProjectContext.GetNamespaceForDomainModel()

            };
            var baseClasses = new[]
            {
                $"BaseRepositoryExtensionsSpecification<{_generationContext.Parameters.EntityRepositoryInterfaceName}>"
            };
            entity = await _roslynGenerator.GeneratePublicClass(
                fullFilename,
                entityNamespace,
                className,
                usingStatements,
                baseClasses
            );

            //var @namespace = await _roslynGenerator.GenerateNamespace(entityNamespace, new[]
            //{
            //    "System",
            //    "Aertssen.Framework.Data.Repositories",

            //    roslynProjectContext.GetNamespaceForDataRepositories(),
            //    roslynProjectContext.GetNamespaceForIntegationTestDataExtensions(),
            //    roslynProjectContext.GetNamespaceForDomainModelHelpers(),
            //    roslynProjectContext.GetNamespaceForDomainModel()
            //});

            //// generate the class
            //entity = await _roslynGenerator.GeneratePublicClass(
            //    fullFilename,
            //    @namespace,
            //    className,
            //    new[]
            //    {
            //        $"BaseRepositoryExtensionsSpecification<{_generationContext.Parameters.EntityRepositoryInterfaceName}>"
            //    }
            //);

           
            //var entityNamespace = _context.GetIntegrationTestsDataNamespace("Data.Extensions");

            //// Generation Code
            //var @namespace = _roslynGenerator.CreateNamespaceDeclaration(entityNamespace)
            //    .AddUsingStatements("System",
            //        _context.GetTestsNamespace("Helpers.Domain"),
            //        _context.GetDomainNamespace("Model"));

            ////  Create a class
            //var classDeclaration = _roslynGenerator.CreateClassDeclaration(className)
            //    .AsPublic()
            //    .AddBaseClasses($"BaseRepositoryExtensionsSpecification<{repositoryInterface}>");


            //// Add the class to the namespace.
            //WriteFile(_context.GetIntegrationTestsDataPath("Repositories", "Queries"), className, @namespace, classDeclaration);
        }
        #endregion

        #region Entity Framework Configuration
        private async Task GenerateEntityMappingConfiguration()
        {
            var className = _generationContext.Parameters.EntityConfigurationName;

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");

            var path = roslynProjectContext.GetPath("Configurations");
            var filename = $"{className}.cs";
            var fullFilename = Path.Combine(path, filename);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity != null)
            {
                return;
            }

            var entityNamespace = roslynProjectContext.GetNamespace("Configurations");

            var usingStatements = new[] {
                "System",
                "Aertssen.Framework.Data.Configurations",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.EntityFrameworkCore.Metadata.Builders",
                roslynProjectContext.GetNamespaceForDomainModel()

            };
            var baseClasses = new[]
            {
                $"AuditableIdentifiableMapping<{_generationContext.Parameters.EntityName}>"
            };
            entity = await _roslynGenerator.GeneratePublicClass(
                fullFilename,
                entityNamespace,
                className,
                usingStatements,
                baseClasses
            );


            //var @namespace = await _roslynGenerator.GenerateNamespace(entityNamespace, new[] { "System",
            //    "Aertssen.Framework.Data.Configurations",
            //    "Microsoft.EntityFrameworkCore",
            //    "Microsoft.EntityFrameworkCore.Metadata.Builders",
            //    roslynProjectContext.GetNamespaceForDomainModel()
            //});

            //// generate the class
            //entity = await _roslynGenerator.GeneratePublicClass(
            //    fullFilename,
            //    @namespace,
            //    className,
            //    new[]
            //    {
            //        $"AuditableIdentifiableMapping<{_generationContext.Parameters.EntityName}>"
            //    }
            //);


            string bodyStatementToTable = $"builder.ToTable(\"{_generationContext.Parameters.PluralEntityName}\");";
            entity = await _roslynGenerator.AppendPublicOverridableMethod(fullFilename, entity, new MethodItem("void", "Configure"),
                new List<PropertyItem>()
                {
                    new PropertyItem($"EntityTypeBuilder <{_generationContext.Parameters.EntityName}>", "builder")
                }, "base.Configure(builder);",
                bodyStatementToTable);
            

        }

        private async Task AppendDatabaseCollectionToDbContext()
        {
            
            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");

            //var path = roslynProjectContext.GetPath("Configurations");
            //var filename = $"{className}.cs";
            //var fullFilename = Path.Combine(path, filename);

            var entity = await roslynProjectContext.GetClassEndingWithName("DbContext");
            if (entity == null)
            {
                throw new InvalidOperationException("Cannot append collection to non-existing (DbContext) class");
            }

            var fullFilename = entity.SyntaxTree.FilePath;

            // TODO Add possibility to add method on specific order in the class !!
            entity = await _roslynGenerator.AppendProperty(fullFilename, entity, new PropertyItem($"DbSet<{_generationContext.Parameters.EntityName}>", _generationContext.Parameters.PluralEntityName));

            //// Get all classes from the project
            ////var classes = await _roslynGenerator.GetclassesFromProject(_context.DataProject) var className = _generationContext.Parameters.EntityConfigurationName;

            ////var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");

            ////var path = roslynProjectContext.GetPath("Configurations");
            ////var filename = $"{className}.cs";
            ////var fullFilename = Path.Combine(path, filename);
            ////ClassDeclarationSyntax databaseContext = classes.FirstOrDefault(x => x.Identifier.ValueText.EndsWith("DbContext"));
            ////if (databaseContext == null)
            ////{
            ////    throw new InvalidOperationException("Cannot append collection to non-existing (DbContext) class");
            ////}

            //var propertyName = $"{entityName}Items";

            ////databaseContext = databaseContext.AddMembers(propertyDeclarationDbSet);
            ////databaseContext.

            //var index = 0;
            //var counter = 0;
            //foreach (var m in databaseContext.Members)
            //{
            //    counter++;
            //    var property = m as PropertyDeclarationSyntax;
            //    if (property != null)
            //    {
            //        var typeSyntax = property.Type.ToFullString();
            //        if (property.Identifier.ValueText == propertyName)
            //            return;

            //        if (typeSyntax.Contains("DbSet"))
            //        {
            //            index = counter;
            //        }
            //    }
            //}


            //var propertyDeclarationDbSet = _roslynGenerator.GenerateProperty($"DbSet<{entityName}>", propertyName);

            //var updatedDatabaseContext = databaseContext.WithMembers(databaseContext.Members.Insert(index, propertyDeclarationDbSet));
            ////var updatedDatabaseContext = databaseContext.AddMembers(propertyDeclarationDbSet);

            //var root = databaseContext.SyntaxTree.GetRoot();
            //var filePath = databaseContext.SyntaxTree.FilePath;

            //var updatedRoot = root.ReplaceNode(databaseContext, updatedDatabaseContext).NormalizeWhitespace();
            //File.WriteAllText(filePath, updatedRoot.ToFullString());


        }
        #endregion

        #region Dto 
        public async Task GenerateEntityActionInterfaceDto()
        {
            var className = _generationContext.Parameters.ActionEntityInterfaceDtoName;

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Dto");

            var path = roslynProjectContext.GetPath(_generationContext.Parameters.PluralEntityName);
            var filename = $"{className}.cs";
            var fullFilename = Path.Combine(path, filename);

            // Generate the entity

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(_generationContext.Parameters.PluralEntityName);
                var usingStatements = new[] {
                    "System",
                };
                var baseClasses = new[]
                {
                    $"IInterfacingDto"
                };
                entity = await _roslynGenerator.GeneratePublicInterface(
                    fullFilename,
                    entityNamespace,
                    className,
                    usingStatements,
                    baseClasses
                );

                //var @namespace = await _roslynGenerator.GenerateNamespace(entityNamespace, new[] { "System"});

                //// generate the class
                //entity = await _roslynGenerator.GeneratePublicInterface(
                //    fullFilename,
                //    @namespace,
                //    className,

                //    new[] { "IInterfacingDto" }
                //);

            }
        }
        public async Task GenerateEntityActionClassDto()
        {
            var className = _generationContext.Parameters.ActionEntityDtoName;

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Dto");

            var path = roslynProjectContext.GetPath(_generationContext.Parameters.PluralEntityName);
            var filename = $"{className}.cs";
            var fullFilename = Path.Combine(path, filename);

            // Generate the entity

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(_generationContext.Parameters.PluralEntityName);

                var usingStatements = new[] {
                    "System",
                };
                var baseClasses = new[]
                {
                    $"{ _generationContext.Parameters.ActionEntityInterfaceDtoName}"
                };
                entity = await _roslynGenerator.GeneratePublicClass(
                    fullFilename,
                    entityNamespace,
                    className,
                    usingStatements,
                    baseClasses
                );


                //var @namespace = await _roslynGenerator.GenerateNamespace(entityNamespace, new[] { "System" });

                //// generate the class
                //entity = await _roslynGenerator.GeneratePublicClass(
                //    fullFilename,
                //    @namespace,
                //    className,
                //    new[] { $"{ _generationContext.Parameters.ActionEntityInterfaceDtoName}" }
                //);

            }
        }
        #endregion

        #region Cqrs 
        public async Task GenerateCqrsInputClass()
        {
            var className = _generationContext.Parameters.CqrsInputClassName;

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Core");

            var path = roslynProjectContext.GetPath("CQRS", _generationContext.Parameters.PluralEntityName, _generationContext.Parameters.ActionName);
            var filename = $"{className}.cs";
            var fullFilename = Path.Combine(path, filename);

            // Generate the entity

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace($"CQRS.{_generationContext.Parameters.PluralEntityName}.{_generationContext.Parameters.ActionName}");

                var usingStatements = new[] {
                    "System",
                };
                var baseClasses = new[]
                {
                    $"{_generationContext.Parameters.CqrsInputType}<{ _generationContext.Parameters.CqrsOutputClassName}>"
                };
                entity = await _roslynGenerator.GeneratePublicClass(
                    fullFilename,
                    entityNamespace,
                    className,
                    usingStatements,
                    baseClasses
                );



                //var @namespace = await _roslynGenerator.GenerateNamespace(entityNamespace, new[] { "System" });

                //// generate the class
                //entity = await _roslynGenerator.GeneratePublicClass(
                //    fullFilename,
                //    @namespace,
                //    className,
                //    new[] { $"{_generationContext.Parameters.CqrsInputType}<{ _generationContext.Parameters.CqrsOutputClassName}>" }
                //);
            }
        }
        #endregion




        //private async Task SetupEntityActionDto(string entityName, string action, string classType)
        //{

        //    //var className = $"{action}{entityName}Dto";
        //    //var interfaceClassName = $"I{className}";

        //    //// Get all classes from the project
        //    //var classes = await _roslynGenerator.GetclassesFromProject(_context.DtoProject);
        //    //var entity = classes.FirstOrDefault(x => x.Identifier.ValueText == className);
        //    //if (entity != null)
        //    //{
        //    //    // Entity already exist in the project, no need to generate it
        //    //    return;
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
        //    var classes = await _roslynGenerator.GetclassesFromProject(_context.CoreProject);
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