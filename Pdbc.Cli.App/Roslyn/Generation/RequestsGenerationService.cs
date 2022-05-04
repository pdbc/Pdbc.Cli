using System.Linq;
using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Model.Items;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation
{
    public class RequestsGenerationService : BaseGenerationService
    {
        public RequestsGenerationService(RoslynSolutionContext roslynSolutionContext,
            FileHelperService fileHelperService,
            GenerationContext context
        ) : base(roslynSolutionContext, fileHelperService, context)
        {
        }

        public async Task Generate()
        {
            await GenerateRequestInputClass();
            await GenerateRequestOutputClass();
        }

        public string[] GetSubFolders()
        {
            return new[] { "Requests", _generationContext.PluralEntityName };
        } 
        public async Task GenerateRequestInputClass()
        {
            var className = _generationContext.RequestInputClassName;
            var subfolders = GetSubFolders();

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Api.Contracts");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            // Generate the entity

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                // TODO Get the correct base class 
                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
                    .AddAertssenFrameworkContractUsingStatements()
                    .AddBaseClass($"AertssenRequest")
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);


            }

            if (_generationContext.RequiresActionDto())
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName(_generationContext.EntityName).ForType(_generationContext.ActionDtoInterface), fullFilename);
            }
        }

        public async Task GenerateRequestOutputClass()
        {
            var className = _generationContext.CqrsOutputClassName;
            var subfolders = GetSubFolders();

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Api.Contracts");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);


            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                // TODO Get the correct base class 
                // List => AertssenListResponse<AssetDataDto> (oData)
                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
                    .AddAertssenFrameworkContractUsingStatements()
                    .AddBaseClass($"AertssenResponse")
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }

            if (_generationContext.StandardActionInfo.RequiresDataDto())
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName(_generationContext.EntityName).ForType(_generationContext.DataDtoClass), fullFilename);
            }
        }
    }
}