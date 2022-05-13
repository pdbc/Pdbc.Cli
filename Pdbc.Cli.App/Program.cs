using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Model;
using Pdbc.Cli.App.Roslyn;
using Pdbc.Cli.App.Roslyn.Generation;
using Pdbc.Cli.App.Services;

namespace Pdbc.Cli.App
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<StartupParameters>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);
        }

        private static void HandleParseError(IEnumerable<Error> obj)
        {
            Console.Write("Cannot parse the startup parameters, going to default parameters");

            var startupParameter = new StartupParameters
            {
                EntityName = "Route",
                PluralEntityName = "Routes",
                Action = "Store"
            };

            RunOptions(startupParameter);
        }

        private static void RunOptions(StartupParameters startupParameters)
        {
            // Dependencies
            var fileHelperService = new FileHelperService();
            
            Console.WriteLine("Starting CLI with the following parameters");
            startupParameters.WriteParameters();

            var path = fileHelperService.GetApplicationDirectory();
            Console.WriteLine($"Reading configuration from '{path}'");

            // Read the configuration + set base path 
            var cliConfiguration = fileHelperService.ReadJsonConfigurationFile<GenerationConfiguration>(path);
            cliConfiguration.BasePath = path;
            cliConfiguration.WriteParameters();
            if (!cliConfiguration.IsValid())
            {
                Console.WriteLine("Generation configuration is not valid, please correct before proceeding");
                return;
            }

            // TODO Fix paths
            cliConfiguration.BasePath = @"C:\repos\Development\Aertssen.Framework.Templates\demo\core1";
            cliConfiguration.ApplicationName = "Locations";
            cliConfiguration.RootNamespace = "Locations";

            // Find the solution (what if we have multiple solutions ??)
            var solutionPath = fileHelperService.GetSolutionPathFrom(cliConfiguration.BasePath);
            Console.WriteLine($"Found the solution to change {solutionPath}");

            var roslySolutionContext = new RoslynSolutionContext(solutionPath, cliConfiguration);
            Console.WriteLine($"Parsed the solution {solutionPath} to setup the workspace");
            
            // Generation Context (how will we handle multipe context (for example LIST/GET/STORE/DELETE in one go)
            var generationContext = new GenerationContext(startupParameters, cliConfiguration);


            var entityGenerationService = new EntityGenerationService(roslySolutionContext, fileHelperService, generationContext);
            entityGenerationService.Generate()
                .GetAwaiter()
                .GetResult();

            var repositoryGenerationService = new RepositoryGenerationService(roslySolutionContext, fileHelperService, generationContext);
            repositoryGenerationService.Generate()
                .GetAwaiter()
                .GetResult();

            var entityFrameworkMappingGenerationService = new EntityFrameworkMappingGenerationService(roslySolutionContext, fileHelperService, generationContext);
            entityFrameworkMappingGenerationService.Generate()
                .GetAwaiter()
                .GetResult();

            var entityFrameworkDbContextGenerationService = new EntityFrameworkDbContextGenerationService(roslySolutionContext, fileHelperService, generationContext);
            entityFrameworkDbContextGenerationService.Generate()
                .GetAwaiter()
                .GetResult();


            if (generationContext.ShouldGenerateCqrs)
            {
                if (generationContext.RequiresActionDto)
                {
                    var entityActionDtoGenerationService = new EntityActionDtoGenerationService(roslySolutionContext, fileHelperService, generationContext);
                    entityActionDtoGenerationService.Generate()
                        .GetAwaiter()
                        .GetResult();
                }

                if (generationContext.RequiresDataDto)
                {
                    var entityDataDtoGenerationService = new EntityDataDtoGenerationService(roslySolutionContext, fileHelperService, generationContext);
                    entityDataDtoGenerationService.Generate()
                        .GetAwaiter()
                        .GetResult();
                }

                var cqrsGenerationService = new CqrsGenerationService(roslySolutionContext, fileHelperService, generationContext);
                cqrsGenerationService.Generate()
                    .GetAwaiter()
                    .GetResult();

                var requestsGenerationService = new RequestsGenerationService(roslySolutionContext, fileHelperService, generationContext);
                requestsGenerationService.Generate()
                    .GetAwaiter()
                    .GetResult();

                var servicesGenerationService = new ServicesGenerationService(roslySolutionContext, fileHelperService, generationContext);
                servicesGenerationService.Generate()
                    .GetAwaiter()
                    .GetResult();

                var integrationTestsGenerationService = new IntegrationTestsGenerationService(roslySolutionContext, fileHelperService, generationContext);
                integrationTestsGenerationService.Generate()
                    .GetAwaiter()
                    .GetResult();
            }

            //var context = new RoslynGenerationContext();
            //var codeGenerationService = new CodeGenerationService(roslySolutionContext,
            //    startupParameters,
            //    cliConfiguration);

            //codeGenerationService.SetupEntity()
            //                     .GetAwaiter()
            //                     .GetResult();
        }
    }

}