using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;
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
                    //.AddUsingStatement(service.GenerationContext.GetNamespaceForCoreCqrs())
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

            entity = await service.AppendUsingStatement(entity, service.GenerationContext.GetNamespaceForCoreCqrs(), fullFilename);

            if (service.GenerationContext.ActionInfo.IsListAction)
            {
                
                entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.OperationNameMethodAsync(service.GenerationContext.ActionInfo.ActionOperationName,
                            service.GenerationContext.ActionInfo.ApiRequestClassName,
                            service.GenerationContext.ActionInfo.ApiResponseClassName)
                        //.WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                        //.Async()
                        //.WithReturnType($"Task<{service.GenerationContext.ActionInfo.ApiResponseClassName}>")
                        //.AddParameter(service.GenerationContext.ActionInfo.ApiRequestClassName, "request")
                        .AddStatement(new StatementSyntaxBuilder(
                            $"return await QueryForOData<{service.GenerationContext.ActionInfo.ApiRequestClassName}, {service.GenerationContext.ActionInfo.CqrsOutputClassNameOverride}, {service.GenerationContext.EntityName.ToDataDto()}, {service.GenerationContext.ActionInfo.ApiResponseClassName}>(request); ")),
                    fullFilename);
                
                //entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                //        .WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                //        .Async()
                //        .WithReturnType($"Task<{service.GenerationContext.ActionInfo.ApiResponseClassName}>")
                //        .AddParameter(service.GenerationContext.ActionInfo.ApiRequestClassName, "request")
                //        .AddStatement(new StatementSyntaxBuilder(
                //            $"return await QueryForOData<{service.GenerationContext.ActionInfo.ApiRequestClassName}, {service.GenerationContext.GetCqrsOutputClassNameBasedOnAction()}, {service.GenerationContext.EntityName.ToDataDto()}, {service.GenerationContext.ActionInfo.ApiResponseClassName}>(request); ")),
                //    fullFilename);
            } else if (service.GenerationContext.ActionInfo.IsGetAction)
            {
                entity = await service.GenerateCqrsStandardQueryMethod(entity, fullFilename);
                
                //entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                //        .WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                //        .Async()
                //        .WithReturnType($"Task<{service.GenerationContext.ActionInfo.ApiResponseClassName}>")
                //        .AddParameter(service.GenerationContext.ActionInfo.ApiRequestClassName, "request")
                //entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.OperationNameMethodAsync(service.GenerationContext.ActionInfo.ActionOperationName,
                //            service.GenerationContext.ActionInfo.ApiRequestClassName,
                //            service.GenerationContext.ActionInfo.ApiResponseClassName)
                //            .AddStatement(new StatementSyntaxBuilder(       
                //            $"return await Query<{service.GenerationContext.ActionInfo.ApiRequestClassName}, {service.GenerationContext.ActionInfo.CqrsInputClassName}, {service.GenerationContext.ActionInfo.CqrsOutputClassName}, {service.GenerationContext.ActionInfo.ApiResponseClassNameOverride}>(request); ")),
                //    fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsDeleteAction)
            {
                entity = await service.GenerateCqrsStandardCommandMethod(entity, fullFilename);
            }
            else if ( service.GenerationContext.ActionInfo.IsStoreAction)
            {
                entity = await service.GenerateCqrsStandardCommandMethod(entity, fullFilename);


                //entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                //        .WithName(service.GenerationContext.ActionInfo.ActionOperationName)
                //        .Async()
                //        .WithReturnType($"Task<{service.GenerationContext.ActionInfo.ApiResponseClassNameOverride}>")
                //        .AddParameter(service.GenerationContext.ActionInfo.ApiRequestClassName, "request")
                //        .AddStatement(new StatementSyntaxBuilder(
                //            $"return await Command<{service.GenerationContext.ActionInfo.ApiRequestClassName}, {service.GenerationContext.ActionInfo.CqrsInputClassName}, {service.GenerationContext.ActionInfo.CqrsOutputClassNameOverride}, {service.GenerationContext.ActionInfo.ApiResponseClassNameOverride}>(request); ")),
                //    fullFilename);
            }

        }
    }
}
