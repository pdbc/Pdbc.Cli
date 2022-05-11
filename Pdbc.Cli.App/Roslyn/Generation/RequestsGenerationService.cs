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
                    .AddUsingStatement(_generationContext.GetNamespaceForDto())
                    .AddAertssenFrameworkContractUsingStatements()
                    .AddBaseClass(_generationContext.ApiRequestBaseClassName)
                    .AddHttpMethodAttribute(_generationContext.GetHttpMethodAttributeValue())
                    .Build();


                ;
                await _fileHelperService.WriteFile(fullFilename, entity);


            }

            if (_generationContext.RequiresActionDto)
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName(_generationContext.EntityName).ForType(_generationContext.ActionDtoInterface), fullFilename);
            }

            if (_generationContext.IsGetAction)
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName("Id").ForType("long"), fullFilename);
            }
        }

        public async Task GenerateRequestOutputClass()
        {
            var className = _generationContext.RequestOutputClassName;
            var subfolders = GetSubFolders();

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Api.Contracts");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);
            
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);


                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(_generationContext.GetNamespaceForDto())
                    .AddAertssenFrameworkContractUsingStatements()
                    .AddBaseClass(_generationContext.ApiResponseBaseClassName)
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }

            if (_generationContext.RequiresDataDto)
            {
                entity = await Save(entity, new PropertyDeclarationSyntaxBuilder().WithName(_generationContext.EntityName).ForType(_generationContext.DataDtoClass), fullFilename);
            }
        }

   
    }
}