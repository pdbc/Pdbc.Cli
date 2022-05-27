using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Dto
{
    public static class EntityDataDtoClassGenerator
    {

        public static async Task GenerateEntityDataClassDto(this GenerationService service)
        {
            var className = service.GenerationContext.EntityName.ToDataDto();
            var subfolders = new[] { service.GenerationContext.PluralEntityName };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Dto");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddBaseClass(service.GenerationContext.EntityName.ToDataDto().ToInterface())
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

        }

        public static async Task GenerateEntityDataClassDtoTestDataBuilder(this GenerationService service)
        {
            var className = service.GenerationContext.EntityName.ToDataDto().ToTestDataBuilder();
            var subfolders = new[] { "DTO", service.GenerationContext.PluralEntityName };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Tests.Helpers");
            var fullFilename = roslynProjectContext.GetFullTestsFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddBaseClass(service.GenerationContext.EntityName.ToDataDto().ToBuilder())
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

        }


        public static async Task AppendEntityToDataDtoMapping(this GenerationService service)
        {
            var className = "DomainDtoMappings";
            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Core");


            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                throw new InvalidOperationException($"Cannot append mappings to non-existing ({className}) class");
            }
            var fullFilename = entity.SyntaxTree.FilePath;


            entity = await service.AppendUsingStatement(entity, service.GenerationContext.GetNamespaceForDomainModel(), fullFilename);
            entity = await service.AppendUsingStatement(entity, service.GenerationContext.GetNamespaceForDto(), fullFilename);

            var setupMethodName = $"Setup{service.GenerationContext.EntityName}Mappings";
            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName(setupMethodName)
                    .AddStatement($"CreateMap<{service.GenerationContext.EntityName}, {service.GenerationContext.EntityName.ToDataDto()}>();")
                , fullFilename);
            

            var constructor = entity.FindConstructorDeclarationSyntaxFor(className);
            if (constructor == null)
            {
                throw new InvalidOperationException($"Cannot append find 'LoadObjects' method");
            }

            var found = false;
            foreach (StatementSyntax n in constructor.Body.Statements)
            {
                var s = n.ToFullString();
                if (s.Contains(setupMethodName))
                {
                    found = true;
                    break;
                }
            }

            // TODO append this method
            if (!found)
            {
                var builder = new StatementSyntaxBuilder($"{setupMethodName}();");
                var s = builder.Build();

                var newMethod = constructor.AddBodyStatements(s);
                var updatedEntity = entity.ReplaceNode(constructor, newMethod);
                await service.SaveAndUpdate(entity, updatedEntity, fullFilename);
            }
            //loadObjectsMethod.Body.Statements
            // Generate Method Setup{PluralEntityName}



        }

    }
}