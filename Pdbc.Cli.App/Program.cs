﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Pdbc.Cli.App.Model;

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
            Console.Write("Cannot parse the startup paraemters, going to default parameters");

            var startupParameter = new StartupParameters();
            startupParameter.EntityName = "Pump";

            RunOptions(startupParameter);
        }

        private static void RunOptions(StartupParameters startupParameters)
        {
            // Dependencies
            var fileHelperService = new FileHelperService();
            
            Console.WriteLine("Starting CLI with the following parameters");
            startupParameters.WriteParameters();

            var path = fileHelperService.GetApplicationDirectory();
            Console.WriteLine($"Reading configuration from {path}");
            var cliConfiguration = fileHelperService.ReadJsonConfigurationFile<CliConfiguration>(path);
            cliConfiguration.WriteParameters();

            // 
            var generationContext = new GenerationContext(path, startupParameters, cliConfiguration);
            if (!generationContext.IsValid())
            {
                Console.WriteLine("Generation context is not valid, please correct before proceeding");
                return;
            }

            // TODO Fix path
            generationContext.BasePath = "C:\\repos\\Development\\IM.Scharnier\\";
            cliConfiguration.ApplicationName = "Scharnier";
            cliConfiguration.RootNamespace = "IM.Scharnier";

            // Find the solution (what if we have multiple solutions ??)
            var solutionPath = fileHelperService.GetSolutionPathFrom(generationContext.BasePath);
            Console.WriteLine($"Found the solution to change {solutionPath}");

            var roslySolutionContext = new RoslynSolutionContext(generationContext, solutionPath);
            Console.WriteLine($"Parsed the solution {solutionPath} to setup the workspace");
            //roslySolutionContext.InitializeProjects();

            //var context = new RoslynGenerationContext();
            var codeGenerationService = new CodeGenerationService(roslySolutionContext, generationContext);
                codeGenerationService.SetupEntity().GetAwaiter().GetResult();


            //await context.Initialize("rootNamespace", "basePath", fileHelperService.GetSolutionPathFrom("basePath"));
            //await context.Initialize(rootNamespace, basePath, fileHelperService.GetSolutionPathFrom(basePath));


            //await codeGenerationService.SetupEntity(generationContext);

            // configuration
            //var basePath= @"C:\repos\Development\IM.Scharnier\";
            //var rootNamespace = "IM.Scharnier";


            //var basePath = @"C:\repos\Development\IM.FuelMgmt\";
            //var rootNamespace = "IM.FuelMgmt";


            // setup context

            // generation

            //await codeGenerationService.SetupEntityAction(entityName, "Store");


        }
    }

}