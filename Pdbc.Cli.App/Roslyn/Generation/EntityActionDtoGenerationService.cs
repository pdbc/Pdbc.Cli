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
    public class EntityActionDtoGenerationService : BaseGenerationService
    {
        public EntityActionDtoGenerationService(RoslynSolutionContext roslynSolutionContext,
            FileHelperService fileHelperService,
            GenerationContext context
        ) : base(roslynSolutionContext, fileHelperService, context)
        {
        }

        public async Task Generate()
        {

            await GenerateEntityActionInterfaceDto();
            await GenerateEntityActionClassDto();
        }

        public async Task GenerateEntityActionInterfaceDto()
        {
            var className = _generationContext.ActionDtoInterface;
            var subfolders = new[] { _generationContext.PluralEntityName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Dto");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                    .AddBaseClass($"IInterfacingDto")
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }


        }

        public async Task GenerateEntityActionClassDto()
        {
            var className = _generationContext.ActionDtoClass;
            var subfolders = new[] { _generationContext.PluralEntityName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Dto");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(_generationContext.GetNamespaceForDomainModel())
                    .AddBaseClass(_generationContext.ActionDtoInterface)
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName("ExternalSystem").ForType("String"), fullFilename);
            entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName("ExternalIdentification").ForType("String"), fullFilename);
            entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName("DateModified").ForType("DateTimeOffset"), fullFilename);

        }
    }
}