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
    public class EntityFrameworkDbContextGenerationService : BaseGenerationService
    {
        public EntityFrameworkDbContextGenerationService(RoslynSolutionContext roslynSolutionContext,
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
            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Data");
            var entity = await roslynProjectContext.GetClassEndingWithName("DbContext");
            if (entity == null)
            {
                throw new InvalidOperationException("Cannot append collection to non-existing (DbContext) class");
            }

            var fullFilename = entity.SyntaxTree.FilePath;

            entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName(_generationContext.PluralEntityName).ForType($"DbSet<{_generationContext.EntityName}>"), fullFilename);

            #region comment AddProperty on specific location in file
            //// Get all classes from the project
            ////var classes = await _roslynGenerator.LoadClassesAndInterfaces(_context.DataProject) var className = _generationContext.Parameters.EntityConfigurationName;

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

            #endregion
        }


    }
}