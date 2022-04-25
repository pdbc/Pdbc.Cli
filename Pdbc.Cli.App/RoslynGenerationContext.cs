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
        public MSBuildWorkspace Workspace { get; set; }

        public RoslynGenerationContext()
        {
            MSBuildLocator.RegisterDefaults();

            Workspace = MSBuildWorkspace.Create();
        }

        //public string SolutionPath { get; set; }
        //public string RootNamespace { get; set; }
        //public string BasePath { get; set; }



        //private Project GetProject(String name)
        //{
        //    return Solution.Projects.FirstOrDefault(x => x.Name.Contains(name));
        //}

        //public async Task Initialize(String rootNamespace, String basePath, string solutionPath)
        //{
        //    //this.BasePath = basePath;
        //    //this.SolutionPath = solutionPath;
        //    //this.RootNamespace = rootNamespace;

        //    MSBuildLocator.RegisterDefaults();
        //    Workspace = MSBuildWorkspace.Create();
        //    Solution = await Workspace.OpenSolutionAsync(SolutionPath);

        //    DomainProject = GetProject("Domain");
        //    DataProject = GetProject("Data");
        //    CoreProject = GetProject("Core");
        //    UnitTestProject = GetProject("UnitTests");
        //    IntegrationDataProject = GetProject("IntegrationTests.Data");
        //    TestHelperProject = GetProject("Tests.Helpers");

        //    DtoProject = GetProject("DTO");
        //    ApiContractsProject = GetProject("Api.Contracts");
        //    CqrsServicesProject = GetProject("Services.Cqrs");
        //}

        public Project CoreProject { get; set; }

        public Project CqrsServicesProject { get; set; }

        public Project ApiContractsProject { get; set; }

        public Project DtoProject { get; set; }


        public Project DomainProject { get; private set; }
        public Project DataProject { get; private set; }


        public Project UnitTestProject { get; private set; }
        public Project IntegrationDataProject { get; private set; }
        public Project TestHelperProject { get; private set; }



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
        public string GetTestsNamespace(String subNamespace = null)
        {
            var result = $"{RootNamespace}.Tests";
            if (subNamespace != null)
            {
                result = $"{result}.{subNamespace}";
            }
            return result;
        }
        public string GetTestHelpersNamespace(String subNamespace = null)
        {
            var result = $"{RootNamespace}.Tests.Helpers";
            if (subNamespace != null)
            {
                result = $"{result}.{subNamespace}";
            }
            return result;
        }
        public string GetCoreNamespace(String subNamespace = null)
        {
            var result = $"{RootNamespace}.Core";
            if (subNamespace != null)
            {
                result = $"{result}.{subNamespace}";
            }
            return result;
        }

        public string GetDtoNamespace(String subNamespace = null)
        {
            var result = $"{RootNamespace}.Dto";
            if (subNamespace != null)
            {
                result = $"{result}.{subNamespace}";
            }
            return result;
        }


        public string GetDomainPath(params String[] subfolders)
        {
            var result = Path.Combine(BasePath, $"{RootNamespace}.Domain");
            return AppendSubfolders(result, subfolders);
        }
        public string GetDataPath(params String[] subfolders)
        {
            var result = Path.Combine(BasePath, $"{RootNamespace}.Data");
            return AppendSubfolders(result, subfolders);
        }

        public string GetUnitTestPath(params String[] subfolders)
        {
            var result = Path.Combine(BasePath, $"{RootNamespace}.UnitTests");
            return AppendSubfolders(result, subfolders); ;
        }
        public string GetIntegrationTestsDataPath(params String[] subfolders)
        {
            var result = Path.Combine(BasePath, $"{RootNamespace}.IntegrationTests.Data");
            return AppendSubfolders(result, subfolders);
        }
        public string GetTestHelpersDataPath(params String[] subfolders)
        {
            var result = Path.Combine(BasePath, $"{RootNamespace}.Tests.Helpers");
            return AppendSubfolders(result, subfolders);
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

        public string GetCorePath(params String[] subfolders)
        {
            var result = Path.Combine(BasePath, $"{RootNamespace}.Core");
            return AppendSubfolders(result, subfolders);
        }

        private String AppendSubfolders(string path, params String[] subfolders)
        {
            EnsureDirectoryExists(path);
            foreach (var subfolder in subfolders)
            {
                path = Path.Combine(path, subfolder);
                EnsureDirectoryExists(path);
            }

            return path;
        }
        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}