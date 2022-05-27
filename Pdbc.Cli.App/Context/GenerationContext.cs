using System;
using Pdbc.Cli.App.Context.Actions;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Model;

namespace Pdbc.Cli.App.Context
{
    public class GenerationContext
    {
        public StartupParameters Parameters { get; }
        private GenerationConfiguration Configuration { get; }

        public GenerationContext(StartupParameters parameters,
                                 GenerationConfiguration configuration)
        {
            Parameters = parameters;
            Configuration = configuration;

            ActionInfo = new ActionInfoFactory().BuildActionInfoFor(this);
        }

        private String _action;
        public void SetAction(String action)
        {
            _action = action;
            ActionInfo = new ActionInfoFactory().BuildActionInfoFor(this);
        }

        public String EntityName => Parameters.EntityName;
        public String PluralEntityName => Parameters.PluralEntityName;
        public String ActionName => _action ?? Parameters.Action;

        public string RootNamespace => Configuration.RootNamespace;
        public String ApplicationName => Configuration.ApplicationName;


        public IActionInfo ActionInfo { get; set; }
        

        public string CqrsServiceContractName => $"{Parameters.PluralEntityName}CqrsService";
        public string WebApiServiceContractName => $"WebApi{Parameters.PluralEntityName}Service";
        

        public string BaseIntegrationTestClass => $"Base{Configuration.ApplicationName}ServiceTest";

        public string BaseCqrsIntegrationTestClass => $"{Configuration.ApplicationName}IntegrationCqrsRequestSpecification";

        public string BaseWebApiIntegrationTestClass => $"{Configuration.ApplicationName}IntegrationApiRequestSpecification";

    }
}