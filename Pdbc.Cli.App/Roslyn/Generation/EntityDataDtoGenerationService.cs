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
    public class EntityDataDtoGenerationService : BaseGenerationService
    {
        public EntityDataDtoGenerationService(RoslynSolutionContext roslynSolutionContext,
            FileHelperService fileHelperService,
            GenerationContext context
        ) : base(roslynSolutionContext, fileHelperService, context)
        {
        }

        public async Task Generate()
        {

            await GenerateEntityDataInterfaceDto();
            await GenerateEntityDataClassDto();
        }

        public async Task GenerateEntityDataInterfaceDto()
        {
            var className = _generationContext.DataDtoInterface;
            var subfolders = new[] { _generationContext.PluralEntityName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Dto");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetInterfaceByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new InterfaceDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }


        }

        public async Task GenerateEntityDataClassDto()
        {
            var className = _generationContext.DataDtoClass;
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
                    .AddBaseClass(_generationContext.DataDtoInterface)
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }

        }
    }
}