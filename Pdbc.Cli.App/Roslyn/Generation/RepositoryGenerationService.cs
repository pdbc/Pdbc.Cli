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
    public class RepositoryGenerationService : GenerationService
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
            var className = GenerationContext.EntityName.ToRepositoryInterface();
            var subfolders = new[] { "Repositories" };

            var roslynProjectContext = RoslynSolutionContext.GetRoslynProjectContextFor("Data");
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
                .AddUsingStatement(GenerationContext.GetNamespaceForDomainModel())
                .AddBaseClass($"IEntityRepository<{GenerationContext.EntityName}>")
                .Build();

            await FileHelperService.WriteFile(fullFilename, entity);


        }

        private async Task GenerateRepositoryClass()
        {
            var className = GenerationContext.EntityName.ToRepository();
            var subfolders = new[] { "Repositories" };

            var roslynProjectContext = RoslynSolutionContext.GetRoslynProjectContextFor("Data");
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
                .AddUsingStatement(GenerationContext.GetNamespaceForDomainModel())
                .AddUsingAertssenFrameworkAuditModel()
                .AddBaseClass($"EntityFrameworkRepository<{GenerationContext.EntityName}>")
                .AddBaseClass(GenerationContext.EntityName.ToRepositoryInterface())
                .Build();

            await FileHelperService.WriteFile(fullFilename, entity);


            entity = await Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter(GenerationContext.DbContextName, "context")
                    .AddBaseParameter("context")
                ,
                fullFilename);

        }

        public async Task GenerateRepositoryBaseIntegrationTests()
        {
            var className = GenerationContext.EntityName.ToRepository()
                                                         .ToSpecification();
            var subfolders = new[] { "Repositories" };


            var roslynProjectContext = RoslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Data");
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
                .AddUsingStatement(GenerationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(GenerationContext.GetNamespaceForDomainModelHelpers())
                .AddUsingAertssenFrameworkAuditModel()
                .AddUnitTestUsingStatement()


                .AddBaseClass($"BaseRepositorySpecification<{GenerationContext.EntityName}>")
                .Build();

            await FileHelperService.WriteFile(fullFilename, entity);


            entity = await Save(entity, new MethodDeclarationSyntaxBuilder().WithName("CreateExistingItem").WithReturnType(GenerationContext.EntityName)
                    .IsOverride(true)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder()
                        .ThatReturnsAndObject($"{GenerationContext.EntityName}TestDataBuilder"))
                    ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder().WithName("CreateNewItem").WithReturnType(GenerationContext.EntityName)
                    .IsOverride(true)
                    .WithModifier(SyntaxKind.ProtectedKeyword)
                    .AddStatement(new StatementSyntaxBuilder()
                        .ThatReturnsAndObject($"{GenerationContext.EntityName}TestDataBuilder"))
                ,
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder().WithName("EditItem")
                    .IsOverride(true)
                    .AddParameter(GenerationContext.EntityName, "item")
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
            var className = GenerationContext.EntityName.ToRepository().ToSpecification("Queries");
            var subfolders = new[] { "Extensions" };

            var roslynProjectContext = RoslynSolutionContext.GetRoslynProjectContextFor("IntegrationTests.Data");
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
                .AddUsingStatement(GenerationContext.GetNamespaceForDataRepositories())
                .AddUsingStatement(GenerationContext.GetNamespaceForIntegationTestDataExtensions())
                .AddUsingStatement(GenerationContext.GetNamespaceForDomainModel())
                .AddUsingStatement(GenerationContext.GetNamespaceForDomainModelHelpers())
                .AddUnitTestUsingStatement()
                .AddUsingAertssenFrameworkAuditModel()
                .AddBaseClass($"BaseRepositoryExtensionsSpecification<{GenerationContext.EntityName.ToRepositoryInterface()}>")
                .Build();

            await FileHelperService.WriteFile(fullFilename, entity);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_extension_queries")
                    .AddTestAttribute(true)
                    .ThrowsNewNotImplementedException(),
                fullFilename);

        }
    }
}
