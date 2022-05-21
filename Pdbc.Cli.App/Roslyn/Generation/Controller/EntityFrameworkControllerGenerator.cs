﻿using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;

namespace Pdbc.Cli.App.Roslyn.Generation.Controller
{
    public static class EntityFrameworkControllerGenerator
    {
        public static async Task GenerateControllerAction(this GenerationService service)
        {

            var className = service.GenerationContext.PluralEntityName.ToController();

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Api", true);
            var subfolders = new[] { "Controllers" };

            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingAertssenFrameworkAuditModel()
                    .AddControllersUsingStatements()
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForCqrsServices())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForRequests())
                    .AddAttribute("ApiController")
                    .AddAttribute(@"Route(""[controller]"")")
                    .AddAttribute("Authorize")

                    .AddBaseClass($"ControllerBase")
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }

            entity = await service.GenerateLoggerVariable(entity, className, fullFilename);
            entity = await service.GenerateCqrsServiceVariable(entity, fullFilename);

            entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                    .AddParameter($"ILogger<{className}>", "logger")
                    .AddParameter(service.GenerationContext.CqrsServiceContractName.ToInterface(), "cqrsService")
                    .AddStatement("_logger = logger;")
                    .AddStatement("_cqrsService = cqrsService;"),
                fullFilename);

            if (service.GenerationContext.ActionInfo.IsListAction)
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName(service.GenerationContext.ActionInfo.PublicActionOperationName)
                        .AddParameter($"[FromQuery] {service.GenerationContext.ActionInfo.RequestInputClassName}", "request")
                        .WithReturnType("Task<IActionResult>")
                        .Async()
                        .AddAttribute("HttpGet")
                        .AddAttribute($"Produces(typeof(IQueryable<{service.GenerationContext.DataDtoClass}>))")
                        .AddAttribute("EnableQueryWithDefaultPageSize")
                        .AddStatement($"var response = await _cqrsService.{service.GenerationContext.ActionInfo.ActionOperationName}(request);")
                        .AddStatement("return Ok(response.Items);"),
                    fullFilename);

            }
        }


    }
}