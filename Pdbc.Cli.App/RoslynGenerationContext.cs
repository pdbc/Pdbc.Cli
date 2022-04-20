using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Pdbc.Cli.App
{
    public class RoslynGenerationContext
    {
        public string SolutionPath { get; set; }
        public string RootNamespace { get; set; }
        public string BasePath { get; set; }

        public MSBuildWorkspace Workspace { get; set; }
        public Solution Solution { get; set; }

        private Project GetProject(String name)
        {
            return Solution.Projects.FirstOrDefault(x => x.Name.Contains(name));
        }

        public async Task Initialize(String rootNamespace, String basePath, string solutionPath)
        {
            this.BasePath = basePath;
            this.SolutionPath = solutionPath;
            this.RootNamespace = rootNamespace;

            MSBuildLocator.RegisterDefaults();
            Workspace = MSBuildWorkspace.Create();
            Solution = await Workspace.OpenSolutionAsync(SolutionPath);

            DomainProject = GetProject("Domain");
            DataProject = GetProject("Data");
            UnitTestProject = GetProject("UnitTests");
            IntegrationDataProject = GetProject("IntegrationTests.Data");
        }

        
        public Project DomainProject { get; private set; }
        public Project DataProject { get; private set; }
        public Project UnitTestProject { get; private set; }
        public Project IntegrationDataProject { get; private set; }


        public string GetDomainNamespace(String subNamespace = null)
        {
            var result = $"{RootNamespace}.Domain";
            if (subNamespace != null)
            {
                result = $"{result}.{subNamespace}";
            }
            return result;
        }
        public string GetDataNamespace(String subNamespace = null)
        {
            var result = $"{RootNamespace}.Data";

            if (subNamespace != null)
            {
                result = $"{result}.{subNamespace}";
            }
            return result;
        }
        public string GetUnitTestNamespace(String subNamespace = null)
        {
            var result = $"{RootNamespace}.UnitTests";

            if (subNamespace != null)
            {
                result = $"{result}.{subNamespace}";
            }
            return result;
        }
        public string GetIntegrationTestsDataNamespace(string subNamespace = null)
        {
            var result = $"{RootNamespace}.IntegrationTests.Data";

            if (subNamespace != null)
            {
                result = $"{result}.{subNamespace}";
            }
            return result;
        }
       
        public string GetDomainPath(params String[] subfolders)
        {
            var result = Path.Combine(BasePath, $"{RootNamespace}.Domain");
            foreach (var subfolder in subfolders)
            {
                result = Path.Combine(result, subfolder);
            }
            return result;
        }
        public string GetDataPath(params String[] subfolders)
        {
            var result = Path.Combine(BasePath, $"{RootNamespace}.Data");
            foreach (var subfolder in subfolders)
            {
                result = Path.Combine(result, subfolder);
            }
            return result;
        }

        public string GetUnitTestPath(params String[] subfolders)
        {
            var result = Path.Combine(BasePath, $"{RootNamespace}.UnitTests");
            foreach (var subfolder in subfolders)
            {
                result = Path.Combine(result, subfolder);
            }

            return result;
        }
        public string GetIntegrationTestsDataPath(params String[] subfolders)
        {
            var result = Path.Combine(BasePath, $"{RootNamespace}.IntegrationTests","Data");
            foreach (var subfolder in subfolders)
            {
                result = Path.Combine(result, subfolder);
            }
            return result;
        }

        //public string GetEntityModelNamespace()
        //{
        //    return $"{GetDomainNamespace()}.Model";
        //}
        //public string GetDataRepositoriesNamespace()
        //{
        //    return $"{GetDataNamespace()}.Repositories";
        //}
        //public string GetDataConfigurationsNamespace()
        //{
        //    return $"{GetDataNamespace()}.Configurations";
        //}


        //public string GetEntityModelPath()
        //{
        //    return Path.Combine(BasePath, $"{RootNamespace}.Domain", "Model");
        //}





        //public string GetEntityRepositoriesPath()
        //{
        //    return Path.Combine(BasePath, $"{RootNamespace}.Data", "Repositories");
        //}
    }
}