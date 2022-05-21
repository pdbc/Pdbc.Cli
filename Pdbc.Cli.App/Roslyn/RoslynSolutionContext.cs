using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Pdbc.Cli.App.Model;

namespace Pdbc.Cli.App.Roslyn
{
    public class RoslynSolutionContext
    {
        private readonly GenerationConfiguration _configuration;
        public MSBuildWorkspace Workspace { get; set; }

        public Solution Solution { get; set; }

        private readonly IDictionary<String, RoslynProjectContext> _roslynProjects;

        public RoslynSolutionContext(String solutionFileName, GenerationConfiguration configuration)
        {
            _configuration = configuration;
            MSBuildLocator.RegisterDefaults();

            Workspace = MSBuildWorkspace.Create();
            Solution = Workspace.OpenSolutionAsync(solutionFileName)
                .GetAwaiter()
                .GetResult();

            _roslynProjects = new Dictionary<string, RoslynProjectContext>();

        }

        public RoslynProjectContext GetRoslynProjectContextFor(String name, bool mustEndsWithName = false)
        {
            RoslynProjectContext roslyContext = null;
            if (!_roslynProjects.TryGetValue(name, out roslyContext))
            {
                var project = GetProject(name, mustEndsWithName);
                roslyContext = new RoslynProjectContext(name, _configuration, project);
                _roslynProjects.Add(name, roslyContext);
            }

            return roslyContext;
        }

        private Project GetProject(String name, bool mustEndsWithName = false)
        {
            if (mustEndsWithName)
            {
                return Solution.Projects.FirstOrDefault(x => x.Name.EndsWith(name));
            }

            return Solution.Projects.FirstOrDefault(x => x.Name.Contains(name));
        }
    }
}