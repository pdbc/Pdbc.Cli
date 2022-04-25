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
        public GenerationContext GenerationContext { get; }
        public Project Project { get; }

        public RoslynProjectContext(string name, GenerationContext generationContext, Project project)
        {
            Name = name;
            GenerationContext = generationContext;
            Project = project;
        }


        public string GetNamespace(String subNamespace = null)
        {
            var result = $"{GenerationContext.Configuration.RootNamespace}.{Name}";
            if (subNamespace != null)
            {
                result = $"{result}.{subNamespace}";
            }
            return result;
        }

        #region Quick Actions

        public string GetNamespaceForDomainModel()
        {
            var result = $"{GenerationContext.Configuration.RootNamespace}.Domain.Model";
            
            return result;
        }
        public string GetNamespaceForDomainModelHelpers()
        {
            var result = $"{GenerationContext.Configuration.RootNamespace}.Tests.Helpers.Domain";

            return result;
        }
        public string GetNamespaceForDataRepositories()
        {
            var result = $"{GenerationContext.Configuration.RootNamespace}.Data.Repositories";

            return result;
        }
        public string GetNamespaceForIntegationTestDataExtensions()
        {
            var result = $"{GenerationContext.Configuration.RootNamespace}.IntegrationTests.Data.Extensions";

            return result;
        }
        #endregion

        public string GetPath(params String[] subfolders)
        {
            var result = Path.Combine(GenerationContext.BasePath, $"{GenerationContext.Configuration.RootNamespace}.{Name}");
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

        // TODO this should go into FileHelperService ???
        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public List<ClassDeclarationSyntax> Classes { get; set; }
    }
}