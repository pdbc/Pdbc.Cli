using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Model.Items;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation
{
    public class ServicesGenerationService : BaseGenerationService
    {
        public ServicesGenerationService(RoslynSolutionContext roslynSolutionContext,
            FileHelperService fileHelperService,
            GenerationContext context
        ) : base(roslynSolutionContext, fileHelperService, context)
        {
        }

        public async Task Generate()
        {
            await GenerateServiceContractInterface();
            await GenerateWebApiServiceContractClass();
            await GenerateCqrsServiceContractClass();
        }

        public string[] GetSubFolders()
        {
            return new[] { "Services", _generationContext.PluralEntityName };
        } 

        public async Task GenerateServiceContractInterface()
        {
            var className = _generationContext.ServiceContractInterfaceName;
            var subfolders = GetSubFolders();

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Api.Contracts");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetInterfaceByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                // TODO Get the correct base class 
                entity = new InterfaceDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForRequests(_generationContext.PluralEntityName))
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
                    .AddAertssenFrameworkContractUsingStatements()
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }


            //Task<ListAssetsResponse> ListAssets(ListAssetsRequest request);
            //entity = await _roslynGenerator.AppendServiceContractMethod(fullFilename, entity, _generationContext);
        }

        public async Task GenerateWebApiServiceContractClass()
        {
            var className = _generationContext.WebApiServiceContractName;
            var subfolders = new[] { _generationContext.PluralEntityName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Api.ServiceAgent");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            // Generate the entity

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForRequests(_generationContext.PluralEntityName))
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForServices(_generationContext.PluralEntityName))
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
                    .AddUsingStatement("Microsoft.Extensions.Logging")
                    .AddAertssenFrameworkServiceAgentsUsingStatements()
                    .AddAertssenFrameworkContractUsingStatements()
                    .AddBaseClass("WebApiService")
                    .AddBaseClass(_generationContext.ServiceContractInterfaceName)
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }


            entity = await Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter("Func<IWebApiClientProxy>", "webApiClientFactory")
                    .AddParameter($"ILogger<{_generationContext.WebApiServiceContractName}>", "logger")
                    .AddBaseParameter("webApiClientFactory")
                    .AddBaseParameter("")
                    .AddBaseParameter("logger")

                   
                ,
                fullFilename);

        }

        public async Task GenerateCqrsServiceContractInterface()
        {
            var className = _generationContext.CqrsServiceContractName.ToInterface();
            var subfolders = new[] { "Services", _generationContext.PluralEntityName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Services.Cqrs");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            // Generate the entity

            var entity = await roslynProjectContext.GetInterfaceByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new InterfaceDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForRequests(_generationContext.PluralEntityName))
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForServices(_generationContext.PluralEntityName))
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
                    .AddAertssenFrameworkContractUsingStatements()
                    
                    .AddBaseClass(_generationContext.ServiceContractInterfaceName)
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
                
            }
            
        }


        public async Task GenerateCqrsServiceContractClass()
        {
            var className = _generationContext.CqrsServiceContractName;
            var subfolders = new[] { "Services", _generationContext.PluralEntityName };

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Services.Cqrs");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            // Generate the entity

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddUsingStatement("MediatR")
                    .AddUsingStatement("AutoMapper")
                    .AddUsingAertssenFrameworkValidationInfra()
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForRequests(_generationContext.PluralEntityName))
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForServices(_generationContext.PluralEntityName))
                    .AddUsingStatement(roslynProjectContext.GetNamespaceForDto(_generationContext.PluralEntityName))
                    .AddAertssenFrameworkContractUsingStatements()

                    .AddBaseClass("CqrsService")
                    .AddBaseClass(_generationContext.CqrsServiceContractName.ToInterface())
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }


            entity = await Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter("IMediator", "mediator")
                    .AddParameter($"IMapper", "mapper")
                    .AddParameter("IValidationBag", "validationBag")
                    .AddBaseParameter("mediator")
                    .AddBaseParameter("mapper")
                    .AddBaseParameter("validationBag")
                ,
                fullFilename);

        }
    }
}