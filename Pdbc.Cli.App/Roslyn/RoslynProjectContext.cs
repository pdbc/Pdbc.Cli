using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Model;

namespace Pdbc.Cli.App.Roslyn
{
    public class RoslynProjectContext
    {
        public string Name { get; }
        
        public GenerationConfiguration Configuration { get; }

        public Project Project { get; }

        public RoslynProjectContext(string name, 
            GenerationConfiguration configuration, 
            Project project)
        {
            Name = name;
            Configuration = configuration;
            Project = project;
        }

        public string GetNamespace(string[] subNamespaces = null)
        {
            var result = $"{Configuration.RootNamespace}.{Name}";
            if (subNamespaces != null)
            {
                var subNamespace = String.Join('.', subNamespaces);
                result = $"{result}.{subNamespace}";
            }
            return result;
        }
        [Obsolete("Prefer the string[] overload")]
        public string GetNamespace(String subNamespace = null)
        {
            var result = $"{Configuration.RootNamespace}.{Name}";
            if (subNamespace != null)
            {
                result = $"{result}.{subNamespace}";
            }
            return result;
        }

        public string GetFullFilenameFor(string className, params String[] subfolders)
        {
            var path = GetPath(subfolders);
            var filename = $"{className}.cs";
            return  Path.Combine(path, filename);
        }
        public string GetFullTestsFilenameFor(string className, params String[] subfolders)
        {
            var path = GetTestsPath(subfolders);
            var filename = $"{className}.cs";
            return Path.Combine(path, filename);
        }

        private string GetTestsPath(params String[] subfolders)
        {
            var result = Path.Combine(Configuration.BasePath, "Tests", $"{Configuration.RootNamespace}.{Name}");
            return AppendSubfolders(result, subfolders);
        }

        private string GetPath(params String[] subfolders)
        {
            var result = Path.Combine(Configuration.BasePath, $"{Configuration.RootNamespace}.{Name}");
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


        #region Quick Actions

        public string GetNamespaceForDomainModel()
        {
            var result = $"{Configuration.RootNamespace}.Domain.Model";

            return result;
        }
        public string GetNamespaceForDomainModelHelpers(string parametersPluralEntityName)
        {
            var result = $"{Configuration.RootNamespace}.Tests.Helpers.Domain.{parametersPluralEntityName}";

            return result;
        }
        public string GetNamespaceForData()
        {
            var result = $"{Configuration.RootNamespace}.Data";

            return result;
        }
        public string GetNamespaceForDataRepositories()
        {
            var result = $"{Configuration.RootNamespace}.Data.Repositories";

            return result;
        }
        //
        public string GetNamespaceForIntegationTestDataExtensions()
        {
            var result = $"{Configuration.RootNamespace}.IntegrationTests.Data.Extensions";

            return result;
        }

        public string GetNamespaceForDto(string parametersPluralEntityName)
        {
            var result = $"{Configuration.RootNamespace}.Dto.{parametersPluralEntityName}";

            return result;
        }
        public string GetNamespaceForRequests(string parametersPluralEntityName)
        {
            var result = $"{Configuration.RootNamespace}.Api.Contracts.Requests.{parametersPluralEntityName}";

            return result;
        }
        public string GetNamespaceForServices(string parametersPluralEntityName)
        {
            var result = $"{Configuration.RootNamespace}.Api.Contracts.Services.{parametersPluralEntityName}";

            return result;
        }
        
        public string GetNamespaceForCoreCqrs(string entityName, String actionName)
        {
            var result = $"{Configuration.RootNamespace}.Core.CQRS.{entityName}.{actionName}";

            return result;
        }

        public string GetNamespaceForCoreCqrsTestDataBuilders(string entityName)
        {
            var result = $"{Configuration.RootNamespace}.Tests.Helpers.CQRS.{entityName}";

            return result;
        }
        #endregion

        // TODO this should go into FileHelperService ???
        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public List<ClassDeclarationSyntax> Classes { get; set; }
        public List<InterfaceDeclarationSyntax> Interfaces { get; set; }



    }
}