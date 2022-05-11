using System;
using Pdbc.Cli.App.Model;

namespace Pdbc.Cli.App.Context
{
    public class StandardAction
    {
        private readonly StartupParameters _startupParameters;
        private readonly string _entityName;
        private readonly string _pluralEntityName;
        private readonly string _action;
        private string _actionName;
        private string _fullActionName;

        public StandardAction(StartupParameters startupParameters) 
        {
            _startupParameters = startupParameters;
            _entityName = startupParameters.EntityName;
            _pluralEntityName = startupParameters.PluralEntityName;
            _action = startupParameters.Action;

            //CalculateActionName();
        }



        

        #region CQRS Type




        #endregion

    }
}