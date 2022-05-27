using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Model;
using Pdbc.Cli.App.Roslyn;
using Pdbc.Cli.App.Roslyn.Generation;
using Pdbc.Cli.App.Roslyn.Generation.Api;
using Pdbc.Cli.App.Roslyn.Generation.Controller;
using Pdbc.Cli.App.Roslyn.Generation.Cqrs;
using Pdbc.Cli.App.Roslyn.Generation.Cqrs.UnitTests;
using Pdbc.Cli.App.Roslyn.Generation.Dto;
using Pdbc.Cli.App.Roslyn.Generation.Entity;
using Pdbc.Cli.App.Roslyn.Generation.IntegrationTests;
using Pdbc.Cli.App.Roslyn.Generation.Services;
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
                EntityName = "Address",
                PluralEntityName = "Addresses",


                //EntityName = "Route",
                //PluralEntityName = "Routes",
                //Action = "List"
                //Action = "Get"
                //Action = "Delete"
                //Action = "Store"
                Action = "CalculateLongitudeLatitude"
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

            //// TODO Fix paths
            //cliConfiguration.BasePath = @"D:\code\Aertssen\aertssen.framework.templates\demo\Sample";
            //cliConfiguration.ApplicationName = "Locations";
            //cliConfiguration.RootNamespace = "Locations";

            // TODO Fix paths
            cliConfiguration.BasePath = @"D:\code\Aertssen\aertssen.framework.templates\demo\Locations";
            cliConfiguration.ApplicationName = "Locations";
            cliConfiguration.RootNamespace = "Locations";

            // Find the solution (what if we have multiple solutions ??)
            var solutionPath = fileHelperService.GetSolutionPathFrom(cliConfiguration.BasePath);
            Console.WriteLine($"Found the solution to change {solutionPath}");

            var roslySolutionContext = new RoslynSolutionContext(solutionPath, cliConfiguration);
            Console.WriteLine($"Parsed the solution {solutionPath} to setup the workspace");
            
            // Generation Context (how will we handle multiple context (for example LIST/GET/STORE/DELETE in one go)
            var generationContext = new GenerationContext(startupParameters, cliConfiguration);

            Generate(roslySolutionContext, fileHelperService, generationContext)
                .GetAwaiter()
                .GetResult();
        }

        private static async Task Generate(RoslynSolutionContext roslySolutionContext,
            FileHelperService fileHelperService, GenerationContext generationContext)
        {
            var generationService = new GenerationService(roslySolutionContext, fileHelperService, generationContext);

            // Entity
            await generationService.GenerateEntity();
            await generationService.GenerateEntityUnitTest();
            await generationService.GenerateEntityTestDataBuilder();

            await generationService.AppendFactoryMethodForDomainEntityToTestCaseService();
            await generationService.AppendFactoryMethodForDomainEntityToTestCaseFactory();
            await generationService.AppendTestDataObjectsMethods();


            // Repository
            await generationService.GenerateRepositoryInterface();
            await generationService.GenerateRepositoryClass();
            await generationService.GenerateRepositoryBaseIntegrationTests();
            await generationService.GenerateRepositoryQueriesIntegrationTests();

            // Entity Framework
            await generationService.GenerateEntityMappingConfiguration();
            await generationService.AppendDbSetToDatabaseContext();

            // Cqrs
            if (generationContext.ActionInfo.ShouldGenerateCqrs)
            {
                if (generationContext.ActionInfo.RequiresActionDto)
                {
                    await generationService.GenerateEntityActionInterfaceDto();
                    await generationService.GenerateEntityActionClassDto();
                    await generationService.GenerateEntityActionClassDtoTestDataBuilder();
                }

                if (generationContext.ActionInfo.RequiresDataDto)
                {
                    await generationService.GenerateEntityDataInterfaceDto();
                    await generationService.GenerateEntityDataClassDto();
                    await generationService.GenerateEntityDataClassDtoTestDataBuilder();
                    await generationService.AppendEntityToDataDtoMapping();
                    //TODO Add tot DomainDtoMappings
                }

                await generationService.GenerateCqrsInputClass();
                await generationService.GenerateCqrsInputClassTestDataBuilder();

                if (generationService.GenerationContext.ActionInfo.ShouldGenerateCqrsOutputClass)
                {
                        await generationService.GenerateCqrsOutputClass();
                }
                await generationService.GenerateCqrsHandlerClass();
                await generationService.GenerateCqrsHandlerUnitTestClass();

                await generationService.GenerateCqrsValidatorClass();
                await generationService.GenerateCqrsValidatorUnitTestClass();

                if (generationService.GenerationContext.ActionInfo.RequiresFactory)
                {
                    await generationService.GenerateCqrsFactoryClass();
                    await generationService.GenerateCqrsFactoryUnitTestClass();
                }

                if (generationService.GenerationContext.ActionInfo.RequiresChangesHandler)
                {
                    await generationService.GenerateCqrsChangesHandlerClass();
                    await generationService.GenerateCqrsChangesHandlerUnitTestClass();
                }


                await generationService.GenerateApiRequestClass();
                await generationService.GenerateApiRequestClassTestDataBuilder();

                if (!generationService.GenerationContext.ActionInfo.IsWithoutResponse)
                {
                    await generationService.GenerateApiResponseClass();
                }

                //await generationService.GenerateApiResponseClassTestDataBuilder();

                await generationService.GenerateServiceContractInterface();
                await generationService.GenerateWebApiServiceContractClass();
                await generationService.GenerateCqrsServiceContractInterface();
                await generationService.GenerateCqrsServiceContractClass();

                await generationService.GenerateBaseServiceIntegrationTest();
                await generationService.GenerateActionSpecificIntegrationTest();
                await generationService.GenerateCqrsIntegrationTest();
                await generationService.GenerateWebApiIntegrationTest();

                await generationService.GenerateControllerAction();

            }
        }

    }

}