using System;

namespace Pdbc.Cli.App.Model
{
    public class CliConfiguration
    {
        public string ApplicationName { get; set; }

        public string RootNamespace { get; set; }

    }

    public static class CliConfigurationExtensions
    {

        public static void WriteParameters(this CliConfiguration configuration)
        {
            Console.WriteLine("Cli Configuration");
            Console.WriteLine($"ApplicationName: {configuration.ApplicationName}");
            Console.WriteLine($"RootNamespace: {configuration.RootNamespace}");
        }
    }
}