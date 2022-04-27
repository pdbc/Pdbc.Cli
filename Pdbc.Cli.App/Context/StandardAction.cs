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

        public StandardAction(StartupParameters startupParameters) 
        {
            _startupParameters = startupParameters;
            _entityName = startupParameters.EntityName;
            _pluralEntityName = startupParameters.PluralEntityName;
            _action = startupParameters.Action;

            //CalculateActionName();
        }

        public String CalculateActionName()
        {
            if (String.IsNullOrEmpty(_action))
                _actionName = String.Empty;

            // TODO calcuate correct Name (ex. StoreWithoutResult = Store , ...)
            _actionName = _action;
            return _actionName;

        }

        public Boolean ShouldGenerateCqrs()
        {
            return _action != null;
        }

        public bool RequiresActionDto()
        {
            if (_actionName is "Store" or "Create" or "Update")
                return true;

            return false;
        }

        public bool RequiresDataDto()
        {
            // TODO if we do not want a response Store/Update/Create should return false!
            if (_actionName is "List" or "Get")
                return true;

            if (_startupParameters.WithoutResponse)
            {
                return false;
            }

            return true;
        }


        #region CQRS Type

        public String GetCqrsInputType()
        {
            if (_actionName is "Get" or "List")
            {
                return "Query";
            }

            return "Command";
        }

        public String GetCqrsOutputType()
        {
            if (_actionName is "Get" or "List")
            {
                return "ViewModel";
            }

            // TODO Can be Nothing
            return "Result";
        }

        #endregion

    }
}