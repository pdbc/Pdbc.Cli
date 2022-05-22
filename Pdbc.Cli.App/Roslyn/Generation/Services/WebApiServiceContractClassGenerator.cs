using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Services
{
    public static class WebApiServiceContractClassGenerator
    {

        public static async Task GenerateWebApiServiceContractClass(this GenerationService service)
        {
            var className = service.GenerationContext.WebApiServiceContractName;
            var subfolders = new[] {service.GenerationContext.PluralEntityName};

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Api.ServiceAgent");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            // Generate the entity

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForRequests())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForServices())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddUsingStatement("Microsoft.Extensions.Logging")
                    .AddAertssenFrameworkServiceAgentsUsingStatements()
                    .AddAertssenFrameworkContractUsingStatements()
                    .AddBaseClass("WebApiService")
                    .AddBaseClass(service.GenerationContext.ServiceContractName.ToInterface())
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter("Func<IWebApiClientProxy>", "webApiClientFactory")
                    .AddParameter($"ILogger<{service.GenerationContext.WebApiServiceContractName}>", "logger")
                    .AddBaseParameter("webApiClientFactory")
                    .AddBaseParameter(service.GenerationContext.PluralEntityName.Quoted())
                    .AddBaseParameter("logger")
                ,
                fullFilename);

            var operationName = service.GenerationContext.ActionInfo.ActionOperationName;
            var requestType = service.GenerationContext.ActionInfo.ApiRequestClassName;
            var resultType = service.GenerationContext.ActionInfo.ApiResponseClassName;

            if (service.GenerationContext.ActionInfo.IsListAction)
            {
                entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.OperationNameMethodAsync(operationName,
                            requestType,
                            resultType)
                        .AddStatement(new StatementSyntaxBuilder(
                            $"return await GetAsyncOData<{requestType}, {resultType}, {service.GenerationContext.EntityName.ToDataDto()}>(request); ")),
                    fullFilename);
            } 
            else if (service.GenerationContext.ActionInfo.IsGetAction)
            {
                entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.OperationNameMethodAsync(operationName,
                            requestType,
                            resultType)
                        .AddStatement(new StatementSyntaxBuilder(
                            $"return await GetAsync<{service.GenerationContext.ActionInfo.ApiResponseClassName}>(request.Id.ToString());")),
                    fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsDeleteAction)
            {

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                        .Async()
                        .WithReturnType($"Task<{service.GenerationContext.ActionInfo.ApiResponseClassNameOverride}>")
                        .AddParameter(service.GenerationContext.ActionInfo.ApiRequestClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder(
                            $"return await DeleteAsync<{service.GenerationContext.ActionInfo.ApiResponseClassNameOverride}>(request.Id.ToString());")),
                    fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsStoreAction)
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                        .Async()
                        .WithReturnType($"Task<{service.GenerationContext.ActionInfo.ApiResponseClassNameOverride}>")
                        .AddParameter(service.GenerationContext.ActionInfo.ApiRequestClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder(
                            $"return await PostAsync<{service.GenerationContext.ActionInfo.ApiRequestClassName},{service.GenerationContext.ActionInfo.ApiResponseClassNameOverride}>(request);")),
                    fullFilename);
            }
        }
    }
}