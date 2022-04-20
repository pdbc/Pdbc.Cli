using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Visitors;

namespace Pdbc.Cli.App
{
    public class CodeGenerationService
    {
        private readonly RoslynGenerationContext _context;
        private readonly RoslynGenerator _roslynGenerator;
        private readonly FileHelperService _fileHelperService;
        

        public CodeGenerationService(RoslynGenerationContext context)
        {
            _context = context;
            _roslynGenerator = new RoslynGenerator();
            _fileHelperService = new FileHelperService();
        }

        /// <summary>
        /// This method generates all what is required to setup a new entity in the solution
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public async Task SetupEntity(String entityName)
        {
            await GenerateEntity(entityName);
            await GenerateEntityUnitTest(entityName);

            await GenerateRepository(entityName);
            await GenerateRepositoryBaseIntegrationTests(entityName);
            await GenerateEntityMappingConfiguration(entityName);
            


            //await GenerateEntityMappingConfiguration(dataProject, "Samsung");
        }


        public async Task GenerateEntity(string entityName)
        {
            // Get all classes from the project
            var classes = await _roslynGenerator.GetclassesFromProject(_context.DomainProject);
            var entity = classes.FirstOrDefault(x => x.Identifier.ValueText == entityName);
            if (entity != null)
            {
                // Entity already exist in the project, no need to generate it
                return;
            }

            var entityNamespace = _context.GetDomainNamespace("Model");

            // Generation Code
            var @namespace = _roslynGenerator.CreateNamespaceDeclaration(entityNamespace)
                .AddUsingStatements("System", "Aertssen.Framework.Audit.Core.Model.Base");
            
            //  Create a class
            var classDeclaration = _roslynGenerator.CreateClassDeclaration(entityName)
                .AsPublic()
                .AddBaseClasses($"BaseEquatableAuditableEntity<{entityName}>", "IInterfacingEntity");


            // Generate Properties
            var propertyDeclarationExternalSystem = _roslynGenerator.GenerateProperty("String", "ExternalSystem");
            var propertyDeclarationExternalIdentification = _roslynGenerator.GenerateProperty("String", "ExternalIdentification");
            var propertyDeclarationDateModified = _roslynGenerator.GenerateProperty("DateTimeOffset", "DateModified");

            // Generate Methods
            var methodShouldAuditPropertyChangeFor = _roslynGenerator.GeneratePublicOverridableMethod("ShouldAuditPropertyChangeFor", "bool")
                .AddMethodParameter("string", "propertyName")
                .AddMethodBody("return true;");

            var methodGetAuditProperties = _roslynGenerator.GeneratePublicOverridableMethod("GetAuditProperties", "IAuditProperties")
                .AddMethodBody(@"
                        return new AuditProperties()
                        {
                            AreaId = this.Id,
                            AreaType = """",
                            ObjectId = this.Id,
                            ObjectType = this.GetType().Name,
                            ObjectInfo = $""""
                        };
                   ");

            // Add properties/methods to class Declaration
            classDeclaration = classDeclaration.AddMembers(
                propertyDeclarationExternalSystem,
                propertyDeclarationExternalIdentification,
                propertyDeclarationDateModified,
                methodShouldAuditPropertyChangeFor,
                methodGetAuditProperties);

            // Add the class to the namespace.
            WriteFile(_context.GetDomainPath("Model"), entityName, @namespace, classDeclaration);
        }

        private async Task GenerateRepository(string entityName)
        {
            var className = $"{entityName}Repository";
            var interfaceName = $"I{className}";

            // Get all classes from the project
            var classes = await _roslynGenerator.GetclassesFromProject(_context.DataProject);
            var entity = classes.FirstOrDefault(x => x.Identifier.ValueText == className);
            if (entity != null)
            {
                // Entity already exist in the project, no need to generate it
                return;
            }

            var entityNamespace = _context.GetDataNamespace("Repositories");

            // Generation Code
            var @namespace = _roslynGenerator.CreateNamespaceDeclaration(entityNamespace)
                .AddUsingStatements("System", 
                    "Aertssen.Framework.Audit.Core.Model.Base", 
                    "Aertssen.Framework.Data.Repositories", 
                    _context.GetDomainPath("Model"));

            //  Create a class: (class Order)
            var interfaceDeclaration = _roslynGenerator.CreateInterfaceDeclaration(interfaceName)
                                                       .AsPublic()
                                                       .AddBaseClasses($"IEntityRepository<{entityName}>");            

            //  Create a class: (class Order)
            var classDeclaration = _roslynGenerator.CreateClassDeclaration(className)
                                                   .AsPublic()
                                                   .AddBaseClasses($"EntityFrameworkRepository<{entityName}>", interfaceName);

            WriteFile(_context.GetDataPath("Repositories"), className, @namespace, interfaceDeclaration, classDeclaration);
        }

        private async Task GenerateEntityMappingConfiguration(string entityName)
        {
            var className = $"{entityName}Configuration";
           
            // Get all classes from the project
            var classes = await _roslynGenerator.GetclassesFromProject(_context.DataProject);
            var entity = classes.FirstOrDefault(x => x.Identifier.ValueText == className);
            if (entity != null)
            {
                // Entity already exist in the project, no need to generate it
                return;
            }

            var entityNamespace = _context.GetDataNamespace("Configurations");

            // Generation Code
            var @namespace = _roslynGenerator.CreateNamespaceDeclaration(entityNamespace)
                .AddUsingStatements("System",
                    "Microsoft.EntityFrameworkCore.Metadata.Builders",
                    "Aertssen.Framework.Data.Configurations",
                    _context.GetDomainNamespace("Model"));

            //  Create a class
            var classDeclaration = _roslynGenerator.CreateClassDeclaration(className)
                .AsPublic()
                .AddBaseClasses($"AuditableIdentifiableMapping<{entityName}>");


            var methodGetAuditProperties = _roslynGenerator.GeneratePublicOverridableMethod("Configure", "void")
                .AddMethodParameter("builder", $"EntityTypeBuilder <{entityName}>")
                .AddMethodBody("base.Configure(builder);");

            WriteFile(_context.GetDataPath("Configurations"), className, @namespace, classDeclaration);
            
        }

        public async Task GenerateEntityUnitTest(string entityName)
        {
            var className = $"{entityName}Specification";
            // Get all classes from the project
            var classes = await _roslynGenerator.GetclassesFromProject(_context.UnitTestProject);
            var entity = classes.FirstOrDefault(x => x.Identifier.ValueText == className);
            if (entity != null)
            {
                // Entity already exist in the project, no need to generate it
                return;
            }

            var entityNamespace = _context.GetUnitTestNamespace("Domain.Model");

            // Generation Code
            var @namespace = _roslynGenerator.CreateNamespaceDeclaration(entityNamespace)
                .AddUsingStatements("System", 
                    "Aertssen.Framework.Tests", 
                    "Aertssen.Framework.Tests.Extensions",
                    "NUnit.Framework",
                    _context.GetDomainNamespace("Model"));

            //  Create a class
            var classDeclaration = _roslynGenerator.CreateClassDeclaration(className)
                .AsPublic()
                .AddBaseClasses("BaseSpecification")
                .AddAttribute("TestFixture");

            
            // Generate Methods
            var unitTestMethod = _roslynGenerator.GeneratePublicMethod("Verify_domain_model_action", "void")
                .AddMethodBody(@"Assert.Fail();")
                .AddAttribute("Test");

            
            // Add properties/methods to class Declaration
            classDeclaration = classDeclaration.AddMembers(
                unitTestMethod);

            // Add the class to the namespace.
            WriteFile(_context.GetUnitTestPath("Domain","Model"), className, @namespace, classDeclaration);
        }

        public async Task GenerateRepositoryBaseIntegrationTests(string entityName)
        {
            var className = $"{entityName}RepositorySpecification";
            // Get all classes from the project
            var classes = await _roslynGenerator.GetclassesFromProject(_context.IntegrationDataProject);
            var entity = classes.FirstOrDefault(x => x.Identifier.ValueText == className);
            if (entity != null)
            {
                // Entity already exist in the project, no need to generate it
                return;
            }

            var entityNamespace = _context.GetIntegrationTestsDataNamespace("Repositories");

            // Generation Code
            var @namespace = _roslynGenerator.CreateNamespaceDeclaration(entityNamespace)
                .AddUsingStatements("System",
                    "Aertssen.Framework.Tests",
                    "Aertssen.Framework.Tests.Extensions",
                    "NUnit.Framework",
                    _context.GetDomainNamespace("Model"));

            //  Create a class
            var classDeclaration = _roslynGenerator.CreateClassDeclaration(className)
                .AsPublic()
                .AddBaseClasses("BaseSpecification")
                .AddAttribute("TestFixture");


            // Generate Methods
            var unitTestMethod = _roslynGenerator.GeneratePublicMethod("Verify_domain_model_action", "void")
                .AddMethodBody(@"Assert.Fail();")
                .AddAttribute("Test");


            // Add properties/methods to class Declaration
            classDeclaration = classDeclaration.AddMembers(
                unitTestMethod);

            // Add the class to the namespace.
            WriteFile(_context.GetUnitTestPath("Domain", "Model"), className, @namespace, classDeclaration);
        }


        private void WriteFile(string path, String entityName, NamespaceDeclarationSyntax @namespace, params MemberDeclarationSyntax[] members)
        {
            //@namespace = @namespace.AddMembers(members);

            var code = GenerateCodenerateCode(@namespace, members);
            _fileHelperService.WriteFile(path, entityName, code);
        }

        private static string GenerateCodenerateCode(NamespaceDeclarationSyntax @namespace, params MemberDeclarationSyntax[] members)
        {
            @namespace = @namespace.AddMembers(members);

            // Normalize and get code as string.
            var code = @namespace
                .NormalizeWhitespace()
                .ToFullString();
            return code;
        }
        
    }
}