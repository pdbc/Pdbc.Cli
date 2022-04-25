using System;

namespace Pdbc.Cli.App.Model
{
    public class GenerationContext
    {
        public string BasePath { get; set; }
        public StartupParameters Parameters { get; }
        public CliConfiguration Configuration { get; }

        public GenerationContext(String basePath, StartupParameters parameters, CliConfiguration configuration)
        {
            BasePath = basePath;
            Parameters = parameters;
            Configuration = configuration;
        }

        public bool IsValid()
        {
            if (Parameters == null || Configuration == null)
                return false;

            if (String.IsNullOrEmpty(Configuration.ApplicationName))
            {
                return false;
            }

            return true;
        }

        //public String SolutionFilename { get; set; }

        //public void InitializeSolutionPath(string solutionPath)
        //{
        //    SolutionFilename = solutionPath;
        //}
    }
}