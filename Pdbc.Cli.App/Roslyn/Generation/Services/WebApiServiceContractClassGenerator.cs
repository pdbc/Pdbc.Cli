using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
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
            var requestType = service.GenerationContext.ActionInfo.RequestInputClassName;
            var resultType = service.GenerationContext.ActionInfo.RequestOutputClassName;
            if (service.GenerationContext.ActionInfo.IsListAction)
            {
                entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.OperationNameMethodAsync(operationName,
                            requestType,
                            resultType)
                       
                        .AddStatement(new StatementSyntaxBuilder().AddStatement(
                            $"return await GetAsyncOData<{requestType}, {resultType}, {service.GenerationContext.DataDtoClass}>(request); ")),
                    fullFilename);
            } 
            else if (service.GenerationContext.ActionInfo.IsGetAction)
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                        .Async()
                        .WithReturnType($"Task<{service.GenerationContext.GetApiOutputClassNameBasedOnAction()}>")
                        .AddParameter(service.GenerationContext.ActionInfo.RequestInputClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder().AddStatement(
                            $"return await GetAsync<{service.GenerationContext.GetApiOutputClassNameBasedOnAction()}>(request.Id.ToString());")),
                    fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsDeleteAction)
            {

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                        .Async()
                        .WithReturnType($"Task<{service.GenerationContext.GetApiOutputClassNameBasedOnAction()}>")
                        .AddParameter(service.GenerationContext.ActionInfo.RequestInputClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder().AddStatement(
                            $"return await DeleteAsync<{service.GenerationContext.GetApiOutputClassNameBasedOnAction()}>(request.Id.ToString());")),
                    fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsStoreAction)
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                        .Async()
                        .WithReturnType($"Task<{service.GenerationContext.GetApiOutputClassNameBasedOnAction()}>")
                        .AddParameter(service.GenerationContext.ActionInfo.RequestInputClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder().AddStatement(
                            $"return await PostAsync<{service.GenerationContext.ActionInfo.RequestInputClassName},{service.GenerationContext.GetApiOutputClassNameBasedOnAction()}>(request);")),
                    fullFilename);
            }
        }
    }
}