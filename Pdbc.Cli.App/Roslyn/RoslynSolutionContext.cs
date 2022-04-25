using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Pdbc.Cli.App.Model;
using Pdbc.Cli.App.Roslyn;

namespace Pdbc.Cli.App
{
    public class RoslynSolutionContext
    {
        private readonly GenerationContext _generationContext;
        public MSBuildWorkspace Workspace { get; set; }

        public Solution Solution { get; set; }
        
        public RoslynSolutionContext(GenerationContext generationContext, 
            String SolutionPath)
        {
            _generationContext = generationContext;
            MSBuildLocator.RegisterDefaults();

            Workspace = MSBuildWorkspace.Create();
            Solution = Workspace.OpenSolutionAsync(SolutionPath)
                .GetAwaiter()
                .GetResult();

            _roslynProjects = new Dictionary<string, RoslynProjectContext>();

        }

        public Project GetProject(String name)
        {
            return Solution.Projects.FirstOrDefault(x => x.Name.Contains(name));
        }

        private IDictionary<String, RoslynProjectContext> _roslynProjects;
        public RoslynProjectContext GetRoslynProjectContextFor(String name)
        {
            RoslynProjectContext roslyContext = null;
            if (!_roslynProjects.TryGetValue(name, out roslyContext))
            {
                var project = GetProject(name);
                roslyContext = new RoslynProjectContext(name, _generationContext, project);
                _roslynProjects.Add(name, roslyContext);
            }

            return roslyContext;
        }

        public void InitializeProjects()
        {
            GetRoslynProjectContextFor("Domain");
            GetRoslynProjectContextFor("Data");
        }

        public RoslynProjectContext ResetProject(string name)
        {
            _roslynProjects.Remove(name);
            return GetRoslynProjectContextFor(name);
        }
    }
}