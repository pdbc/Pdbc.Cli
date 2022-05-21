using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App.Roslyn.Generation.Services
{
    public static class CqrsServiceContractClassGenerator
    {
        public static async Task GenerateCqrsServiceContractClass(this GenerationService service)
        {
            var className = service.GenerationContext.CqrsServiceContractName;
            var subfolders = new[] {"Services", service.GenerationContext.PluralEntityName};

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Services.Cqrs");
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
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForRequests())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForServices())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForCoreCqrs())
                    .AddUsingAertssenFrameworkCqrsServices()
                    .AddAertssenFrameworkContractUsingStatements()

                    .AddBaseClass("CqrsService")
                    .AddBaseClass(service.GenerationContext.CqrsServiceContractName.ToInterface())
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }


            entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter("IMediator", "mediator")
                    .AddParameter($"IMapper", "mapper")
                    .AddParameter("IValidationBag", "validationBag")
                    .AddBaseParameter("mediator")
                    .AddBaseParameter("mapper")
                    .AddBaseParameter("validationBag")
                ,
                fullFilename);


            if (service.GenerationContext.ActionInfo.IsListAction)
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                        .Async()
                        .WithReturnType($"Task<{service.GenerationContext.ActionInfo.RequestOutputClassName}>")
                        .AddParameter(service.GenerationContext.ActionInfo.RequestInputClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder().AddStatement(
                            $"return await QueryForOData<{service.GenerationContext.ActionInfo.RequestInputClassName}, {service.GenerationContext.GetCqrsOutputClassNameBasedOnAction()}, {service.GenerationContext.DataDtoClass}, {service.GenerationContext.ActionInfo.RequestOutputClassName}>(request); ")),
                    fullFilename);
            } else if (service.GenerationContext.ActionInfo.IsGetAction)
            {
                 entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                        .Async()
                        .WithReturnType($"Task<{service.GenerationContext.GetApiOutputClassNameBasedOnAction()}>")
                        .AddParameter(service.GenerationContext.ActionInfo.RequestInputClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder().AddStatement(
                            $"return await Query<{service.GenerationContext.ActionInfo.RequestInputClassName}, {service.GenerationContext.ActionInfo.CqrsInputClassName}, {service.GenerationContext.ActionInfo.CqrsOutputClassName}, {service.GenerationContext.GetApiOutputClassNameBasedOnAction()}>(request); ")),
                    fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsDeleteAction || service.GenerationContext.ActionInfo.IsStoreAction)
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                        .Async()
                        .WithReturnType($"Task<{service.GenerationContext.GetApiOutputClassNameBasedOnAction()}>")
                        .AddParameter(service.GenerationContext.ActionInfo.RequestInputClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder().AddStatement(
                            $"return await Command<{service.GenerationContext.ActionInfo.RequestInputClassName}, {service.GenerationContext.ActionInfo.CqrsInputClassName}, {service.GenerationContext.GetCqrsOutputClassNameBasedOnAction()}, {service.GenerationContext.GetApiOutputClassNameBasedOnAction()}>(request); ")),
                    fullFilename);
            }

        }
    }
}
