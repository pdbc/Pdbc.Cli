using System;

namespace Pdbc.Cli.App.Model
{
    public class GenerationConfiguration
    {
        public string ApplicationName { get; set; }

        public string RootNamespace { get; set; }

        public String BasePath { get; set; }

    }

    public static class CliConfigurationExtensions
    {

        public static void WriteParameters(this GenerationConfiguration configuration)
        {
            Console.WriteLine("Cli Configuration");
            Console.WriteLine($"ApplicationName: {configuration.ApplicationName}");
            Console.WriteLine($"RootNamespace: {configuration.RootNamespace}");
            Console.WriteLine($"BasePath: {configuration.BasePath}");
        }

        public static Boolean IsValid(this GenerationConfiguration configuration)
        {
            if (String.IsNullOrEmpty(configuration.ApplicationName))
            {
                Console.WriteLine("ApplicationName is required");
                return false;
            }
            if (String.IsNullOrEmpty(configuration.RootNamespace))
            {
                Console.WriteLine("RootNamespace is required");
                return false;
            }
            if (String.IsNullOrEmpty(configuration.BasePath))
            {
                Console.WriteLine("BasePath is required");
                return false;
            }

            return true;
        }
    }
}