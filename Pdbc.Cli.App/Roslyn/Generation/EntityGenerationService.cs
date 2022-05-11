using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Model.Items;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation
{
    public class EntityGenerationService : BaseGenerationService
    {
        public EntityGenerationService(RoslynSolutionContext roslynSolutionContext,
                FileHelperService fileHelperService,
                GenerationContext context
            ) : base(roslynSolutionContext, fileHelperService, context)
        {
        }

        public async Task Generate()
        {
            await GenerateEntity();
            await GenerateEntityUnitTest();
            await GenerateTestDataBuilder();
        }

        public async Task GenerateEntity()
        {
            var className = _generationContext.EntityName;
            var subfolders = new[] { "Model" };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Domain");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement("System")
                    .AddUsingAertssenFrameworkAuditModel()
                    .AddBaseClass($"BaseEquatableAuditableEntity<{className}>")
                    .AddBaseClass("IInterfacingEntity")
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }
            
            entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName("ExternalSystem").ForType("String"), fullFilename);
            entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName("ExternalIdentification").ForType("String"), fullFilename);
            entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName("DateModified").ForType("DateTimeOffset"), fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("ShouldAuditPropertyChangeFor")
                    .WithReturnType("bool")
                    .IsOverride(true)
                    .AddParameter("string", "propertyName")
                    .ReturnsTrue(), 
                fullFilename);

            entity = await Save(entity, new MethodDeclarationSyntaxBuilder().WithName("GetAuditProperties").WithReturnType("IAuditProperties")
                    .IsOverride(true)
                    .AddStatement(new StatementSyntaxBuilder()
                        .AddStatement(@"
                                return new AuditProperties()
                                {
                                    AreaId = this.Id,
                                    AreaType = this.GetType().Name,
                                    ObjectId = this.Id,
                                    ObjectType = this.GetType().Name,
                                    ObjectInfo = $""""
                               };
                            "))
                    ,
                fullFilename);

                //.WithBody(
                

        }

        public async Task GenerateEntityUnitTest()
        {
            var className = _generationContext.EntityName.ToSpecification();
            var subfolders = new[] { "Domain", "Model" };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("UnitTests");
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
                .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                .AddUsingAertssenFrameworkAuditModel()
                .AddUnitTestUsingStatement()
                .AddBaseClass($"BaseSpecification")
                .AddTestFixtureAttribute(true)
                .Build();

            await _fileHelperService.WriteFile(fullFilename, entity);


            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("Verify_domain_model_action")
                    .AddTestAttribute(true)
                    .ThrowsNewNotImplementedException(),
                fullFilename);

        }

        public async Task GenerateTestDataBuilder()
        {
            var className = _generationContext.EntityName.ToTestDataBuilder();
            var subfolders = new[] { "Domain", _generationContext.PluralEntityName };

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
                .AddAertssenFrameworkCoreUsingStatements()
                .AddUnitTestUsingStatement()
                .AddBaseClass(_generationContext.EntityName.ToBuilder())
                .AddBaseClass($"IExternallyIdentifiableObjectBuilder<{_generationContext.EntityName.ToBuilder()}>")
                .Build();

            await _fileHelperService.WriteFile(fullFilename, entity);
            

        }
    }
}
