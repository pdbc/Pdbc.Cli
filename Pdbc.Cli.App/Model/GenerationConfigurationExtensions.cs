using System;

namespace Pdbc.Cli.App.Model
{
    public static class GenerationConfigurationExtensions
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