﻿using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Entity
{
    public static class EntityFrameworkMappingGenerator
    {
        public static async Task GenerateEntityMappingConfiguration(this GenerationService service)
        {
            var className = service.GenerationContext.EntityName.ToEntityConfigurationClass();
            var subfolders = new[] { "Configurations" };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Data");
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
                .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                .AddBaseClass($"AuditableIdentifiableMapping<{service.GenerationContext.EntityName}>")
                .Build();

            await service.FileHelperService.WriteFile(fullFilename, entity);

            string bodyStatementToTable = $"builder.ToTable(\"{service.GenerationContext.PluralEntityName}\");";
            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder().WithName("Configure")
                    .AddParameter($"EntityTypeBuilder <{service.GenerationContext.EntityName}>", "builder")
                    .IsOverride(true)
                    .AddStatement(new StatementSyntaxBuilder().AddStatement(@"base.Configure(builder);"))
                    .AddStatement(new StatementSyntaxBuilder().AddStatement(bodyStatementToTable))
                ,
                fullFilename);
        }

    }
}