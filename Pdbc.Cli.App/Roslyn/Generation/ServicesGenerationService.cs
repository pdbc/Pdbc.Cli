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
            await GenerateCqrsServiceContractInterface();
            await GenerateCqrsServiceContractClass();
        }

        public string[] GetSubFolders()
        {
            return new[] {"Services", _generationContext.PluralEntityName};
        }


        public async Task GenerateServiceContractInterface()
        {
            var className = _generationContext.ServiceContractName.ToInterface();
            var subfolders = GetSubFolders();

            var roslynProjectContext = _roslynSolutionContext.GetRoslynProjectContextFor("Api.Contracts");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            // Generate the entity
            var entity = await roslynProjectContext.GetInterfaceByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new InterfaceDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(_generationContext.GetNamespaceForRequests())
                    .AddUsingStatement(_generationContext.GetNamespaceForDto())
                    .AddAertssenFrameworkContractUsingStatements()
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }


            entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName(_generationContext.ActionOperationName)
                    .IsInterfaceMethod(true)
                    .WithReturnType($"Task<{_generationContext.RequestOutputClassName}>")
                    .AddParameter(_generationContext.RequestInputClassName, "request"),
                fullFilename);
        }

        public async Task GenerateWebApiServiceContractClass()
        {
            var className = _generationContext.WebApiServiceContractName;
            var subfolders = new[] {_generationContext.PluralEntityName};

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
                    .AddUsingStatement(_generationContext.GetNamespaceForRequests())
                    .AddUsingStatement(_generationContext.GetNamespaceForServices())
                    .AddUsingStatement(_generationContext.GetNamespaceForDto())
                    .AddUsingStatement("Microsoft.Extensions.Logging")
                    .AddAertssenFrameworkServiceAgentsUsingStatements()
                    .AddAertssenFrameworkContractUsingStatements()
                    .AddBaseClass("WebApiService")
                    .AddBaseClass(_generationContext.ServiceContractName.ToInterface())
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter("Func<IWebApiClientProxy>", "webApiClientFactory")
                    .AddParameter($"ILogger<{_generationContext.WebApiServiceContractName}>", "logger")
                    .AddBaseParameter("webApiClientFactory")
                    .AddBaseParameter(_generationContext.PluralEntityName.Quoted())
                    .AddBaseParameter("logger")
                ,
                fullFilename);

            if (_generationContext.IsListAction)
            {
                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(_generationContext.ActionOperationName)
                        .Async()
                        .WithReturnType($"Task<{_generationContext.RequestOutputClassName}>")
                        .AddParameter(_generationContext.RequestInputClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder().AddStatement(
                            $"return await GetAsyncOData<{_generationContext.RequestInputClassName}, {_generationContext.RequestOutputClassName}, {_generationContext.DataDtoClass}>(request); ")),
                    fullFilename);
            } 
            else if (_generationContext.IsGetAction)
            {
                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(_generationContext.ActionOperationName)
                        .Async()
                        .WithReturnType($"Task<{_generationContext.RequestOutputClassName}>")
                        .AddParameter(_generationContext.RequestInputClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder().AddStatement(
                            $"return await GetAsync<{_generationContext.RequestOutputClassName}>(request.Id.ToString());")),
                    fullFilename);

                //public async Task<GetAssetResponse> GetAsset(GetAssetRequest request)
                //{
                //    return await GetAsync<GetAssetResponse>($"{request.Id}");
                //}
            }
        }

        public async Task GenerateCqrsServiceContractInterface()
        {
            var className = _generationContext.CqrsServiceContractName.ToInterface();
            var subfolders = new[] {"Services", _generationContext.PluralEntityName};

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
                    .AddUsingStatement(_generationContext.GetNamespaceForRequests())
                    .AddUsingStatement(_generationContext.GetNamespaceForServices())
                    .AddUsingStatement(_generationContext.GetNamespaceForDto())
                    .AddAertssenFrameworkContractUsingStatements()

                    .AddBaseClass(_generationContext.ServiceContractName.ToInterface())
                    .Build();

                await _fileHelperService.WriteFile(fullFilename, entity);

            }

        }

        public async Task GenerateCqrsServiceContractClass()
        {
            var className = _generationContext.CqrsServiceContractName;
            var subfolders = new[] {"Services", _generationContext.PluralEntityName};

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
                    .AddUsingStatement(_generationContext.GetNamespaceForRequests())
                    .AddUsingStatement(_generationContext.GetNamespaceForServices())
                    .AddUsingStatement(_generationContext.GetNamespaceForDto())
                    .AddUsingStatement(_generationContext.GetNamespaceForCoreCqrs())
                    .AddUsingAertssenFrameworkCqrsServices()
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


            if (_generationContext.IsListAction)
            {
                entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(_generationContext.ActionOperationName)
                        .Async()
                        .WithReturnType($"Task<{_generationContext.RequestOutputClassName}>")
                        .AddParameter(_generationContext.RequestInputClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder().AddStatement(
                            $"return await QueryForOData<{_generationContext.RequestInputClassName}, {_generationContext.CqrsInputClassName}, {_generationContext.DataDtoClass}, {_generationContext.RequestOutputClassName}>(request); ")),
                    fullFilename);
            } else if (_generationContext.IsGetAction)
            {
                 entity = await Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(_generationContext.ActionOperationName)
                        .Async()
                        .WithReturnType($"Task<{_generationContext.RequestOutputClassName}>")
                        .AddParameter(_generationContext.RequestInputClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder().AddStatement(
                            $"return await Query<{_generationContext.RequestInputClassName}, {_generationContext.CqrsInputClassName}, {_generationContext.CqrsOutputClassName}, {_generationContext.RequestOutputClassName}>(request); ")),
                    fullFilename);
            }

        }
    }
}
