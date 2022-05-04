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
    public class EntityFrameworkMappingGenerationService : BaseGenerationService
    {
        public EntityFrameworkMappingGenerationService(RoslynSolutionContext roslynSolutionContext,
            FileHelperService fileHelperService,
            GenerationContext context
        ) : base(roslynSolutionContext, fileHelperService, context)
        {
        }

        public async Task Generate()
        {
            await GenerateEntityMappingConfiguration();
           
        }

        public async Task GenerateEntityMappingConfiguration()
        {
            var className = _generationContext.EntityName.ToEntityConfigurationClass();
            var subfolders = new[] { "Configurations" };

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
                .AddUsingAertssenFrameworkConfiguration()
                .AddUsingAertssenFrameworkAuditModel()
                .AddUsingAertssenFrameworkRepositories()
                .AddUsingStatement(roslynProjectContext.GetNamespaceForDomainModel())
                .AddBaseClass($"AuditableIdentifiableMapping<{_generationContext.EntityName}>")
                .Build();

            await _fileHelperService.WriteFile(fullFilename, entity);

            string bodyStatementToTable = $"builder.ToTable(\"{_generationContext.PluralEntityName}\");";
            entity = await Save(entity, new MethodDeclarationSyntaxBuilder().WithName("Configure")
                    .AddParameter($"EntityTypeBuilder <{_generationContext.EntityName}>","builder")
                    .AddStatement(new StatementSyntaxBuilder().AddStatement(bodyStatementToTable))
                ,
                fullFilename);

        }

    }
}