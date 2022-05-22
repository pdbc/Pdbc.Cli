using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.ExpressionBuilders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation.Entity
{
    public static class EntityGenerator
    {
        public static async Task GenerateEntity(this GenerationService service)
        {
            var className = service.GenerationContext.EntityName;
            var subfolders = new[] { "Model" };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Domain");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingAertssenFrameworkAuditModel()
                    .AddBaseClass($"BaseEquatableAuditableEntity<{className}>")
                    .AddBaseClass("IInterfacingEntity")
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await service.GenerateExternalSystemProperty(entity, fullFilename);
            entity = await service.GenerateExternalIdentificationProperty(entity, fullFilename);
            entity = await service.GenerateDateModifiedProperty(entity, fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName("ShouldAuditPropertyChangeFor")
                    .WithReturnType("bool")
                    .IsOverride(true)
                    .AddParameter("string", "propertyName")
                    .ReturnsTrue(),
                fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder().WithName("GetAuditProperties").WithReturnType("IAuditProperties")
                    .IsOverride(true)
                    .AddStatement(new ReturnStatementSyntaxBuilder().WithObjectCreationExpression(
                        new ObjectCreationExpressionSyntaxBuilder("AuditProperties")
                            .AddAssignementStatement("AreaId", "this.Id")
                            .AddAssignementStatement("AreaType", "this.GetType().Name")
                            .AddAssignementStatement("ObjectId", "this.Id")
                            .AddAssignementStatement("ObjectType", "this.GetType().Name")
                            .AddAssignementStatement("ObjectInfo", "String.Empty"))
                    ),
                fullFilename);
        }

    }
}
