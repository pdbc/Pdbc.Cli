using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation
{
    public class RepositoryGenerationService : BaseGenerationService
    {
        public RepositoryGenerationService(RoslynSolutionContext roslynSolutionContext,
                FileHelperService fileHelperService,
                GenerationContext context
            ) : base(roslynSolutionContext, fileHelperService, context)
        {
        }

        public async Task Generate()
        {
            await GenerateRepositoryInterface();
            await GenerateRepositoryClass();

            await GenerateRepositoryBaseIntegrationTests();
            await GenerateRepositoryQueriesIntegrationTests();
        }

        public async Task GenerateRepositoryInterface()
        {
            var className = _generationContext.EntityName.ToRepositoryInterface();
            var subfolders = new[] { "Repositories" };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetInterfaceByName(className);
            if (entity != null)
            {
                return;
            }


            var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

            entity = new InterfaceDeclarationSyntaxBuilder()
                .WithName(className)
                .ForNamespace(entityNamespace)
                .AddUsingStatement("System")
                .AddUsingAertssenFrameworkAuditModel()
                .AddUsingAertssenFrameworkRepositories()
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddBaseClass($"IEntityRepository<{_generationContext.EntityName}>")
                .Build();

            await _fileHelperService.WriteFile(fullFilename, entity);


        }

        private async Task GenerateRepositoryClass()
        {
            var className = _generationContext.EntityName.ToRepository();
            var subfolders = new[] { "Repositories" };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

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
                .AddUsingAertssenFrameworkRepositories()
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddUsingAertssenFrameworkAuditModel()
                .AddBaseClass($"EntityFrameworkRepository<{_generationContext.EntityName}>")
                .AddBaseClass(_generationContext.EntityName.ToRepositoryInterface())
                .Build();

            await _fileHelperService.WriteFile(fullFilename, entity);


            entity = await Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter(_generationContext.DbContextName, "context")
                    .AddBaseParameter("context")
                ,
                fullFilename);

        }

        public async Task GenerateRepositoryBaseIntegrationTests()
        {
            var className = _generationContext.EntityName.ToRepository()
                                                         .ToSpecification();
            var subfolders = new[] { "Repositories" };


            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Data");
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
                .AddUsingAertssenFrameworkRepositories()
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModelHelpers())
                .AddUsingAertssenFrameworkAuditModel()
                .AddUnitTestUsingStatement()


                .AddBaseClass($"BaseRepositorySpecification<{_generationContext.EntityName}>")
                .Build();

            await _fileHelperService.WriteFile(fullFilename, entity);


            entity = await Save(entity, new MethodDeclarationSyntaxBuilder().WithName("CreateExistingItem").WithReturnType(_generationContext.EntityName)
                    .IsOverride(true)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder()
                        .ThatReturnsAndObject($"{_generationContext.EntityName}TestDataBuilder"))
                    ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder().WithName("CreateNewItem").WithReturnType(_generationContext.EntityName)
                    .IsOverride(true)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder()
                        .ThatReturnsAndObject($"{_generationContext.EntityName}TestDataBuilder"))
                ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder().WithName("EditItem")
                    .IsOverride(true)
                    .AddParameter(_generationContext.EntityName, "item")
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .ThrowsNewNotImplementedException()
                ,
                fullFilename);

            //entity = await _roslynGenerator.AppendProtectedOverridableMethod(fullFilename, entity,
            //    new MethodItem(_generationContext.EntityName, "CreateExistingItem"),
            //    new List<PropertyItem>(), $"return new {_generationContext.EntityName}TestDataBuilder();");
            //entity = await _roslynGenerator.AppendProtectedOverridableMethod(fullFilename, entity,
            //    new MethodItem(_generationContext.EntityName, "CreateNewItem"),
            //    new List<PropertyItem>(), $"return new {_generationContext.EntityName}TestDataBuilder();");
            //entity = await _roslynGenerator.AppendProtectedOverridableMethod(fullFilename, entity,
            //    new MethodItem("void", "EditItem"),
            //    new List<PropertyItem>()
            //    {
            //        new PropertyItem(_generationContext.EntityName, "item")
            //    }, $"throw new NotImplementedException();");
        }

        public async Task GenerateRepositoryQueriesIntegrationTests()
        {
            var className = _generationContext.EntityName.ToRepository().ToSpecification("Queries");
            var subfolders = new[] { "Extensions" };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Data");
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
                
                .AddUsingAertssenFrameworkRepositories()
                .AddUsingStatement(_generationContext.GetNamespaceForDataRepositories())
                .AddUsingStatement(_generationContext.GetNamespaceForIntegationTestDataExtensions())
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModelHelpers())
                .AddUnitTestUsingStatement()
                .AddUsingAertssenFrameworkAuditModel()
                .AddBaseClass($"BaseRepositoryExtensionsSpecification<{_generationContext.EntityName.ToRepositoryInterface()}>")
                .Build();

            await _fileHelperService.WriteFile(fullFilename, entity);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_extension_queries")
                    .AddTestAttribute(true)
                    .ThrowsNewNotImplementedException(),
                fullFilename);

        }
    }
}
