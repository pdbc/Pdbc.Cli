using System;

namespace Pdbc.Cli.App.Context.Actions
{
    public interface IActionInfo
    {
        //string ActionName { get; }
        String ActionEntityName { get; }
        
        String EntityActionName { get; }


        
        #region Base Classes
        string CqrsInputType { get; }
        string CqrsOutputType { get; }
        

        String ApiRequestBaseClassName { get; }
        String ApiResponseBaseClassName { get; }
        #endregion

        string ActionOperationName { get; }

        string ApiRequestClassName { get; }
        string ApiResponseClassName { get; }

        string CqrsInputClassName { get; }
        string CqrsOutputClassName { get; }

        string ServiceContractName { get; set; }

        #region Possible Overrides
        string PublicActionOperationName { get; }

        string CqrsOutputClassNameOverride { get; set; }
        string ApiResponseClassNameOverride { get; set; }

        #endregion


        Boolean ShouldGenerateCqrs { get; }
        Boolean ShouldGenerateCqrsOutputClass { get; }
        

        Boolean RequiresActionDto { get; }
        Boolean RequiresDataDto { get; }
        
        Boolean RequiresFactory { get; set; }
        Boolean RequiresChangesHandler { get; set; }


        //Boolean IsQueryAction { get; }
        //Boolean IsCommandAction { get; }
        
        


        Boolean IsListAction { get; }
        Boolean IsGetAction { get; }
        Boolean IsDeleteAction { get; }
        Boolean IsStoreAction { get; }
        Boolean IsUpdateAction { get; }
        Boolean IsCreateAction { get; }
        Boolean IsWithoutResponse { get; }



        String HttpMethodAttributeUrl { get; set; }

        String HttpMethodAttributeMethod { get; set; }
    }
}