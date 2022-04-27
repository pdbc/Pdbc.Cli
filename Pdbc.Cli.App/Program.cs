using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Pdbc.Cli.App.Model;
using Pdbc.Cli.App.Roslyn;
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
            Console.Write("Cannot parse the startup paraemters, going to default parameters");

            var startupParameter = new StartupParameters
            {
                EntityName = "Address",
                PluralEntityName = "Addresses",
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
            cliConfiguration.BasePath = @"C:\repos\Development\Aertssen.Framework.Templates\demo\core";
            cliConfiguration.ApplicationName = "Locations";
            cliConfiguration.RootNamespace = "Locations";

            // Find the solution (what if we have multiple solutions ??)
            var solutionPath = fileHelperService.GetSolutionPathFrom(cliConfiguration.BasePath);
            Console.WriteLine($"Found the solution to change {solutionPath}");

            var roslySolutionContext = new RoslynSolutionContext(solutionPath, cliConfiguration);
            Console.WriteLine($"Parsed the solution {solutionPath} to setup the workspace");
            
            //var context = new RoslynGenerationContext();
            var codeGenerationService = new CodeGenerationService(roslySolutionContext, 
                startupParameters, 
                cliConfiguration);

            codeGenerationService.SetupEntity()
                                 .GetAwaiter()
                                 .GetResult();
        }
    }

}