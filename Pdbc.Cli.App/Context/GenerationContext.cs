using System;
using Pdbc.Cli.App.Context.Actions;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Model;

namespace Pdbc.Cli.App.Context
{
    public class GenerationContext
    {
        private StartupParameters Parameters { get; }
        private GenerationConfiguration Configuration { get; }

        public GenerationContext(StartupParameters parameters,
                                 GenerationConfiguration configuration)
        {
            Parameters = parameters;
            Configuration = configuration;

            BuildActionInfo();
        }

        
        public String EntityName => Parameters.EntityName;
        public String PluralEntityName => Parameters.PluralEntityName;
        public String ActionName => Parameters.Action;

        public string RootNamespace => Configuration.RootNamespace;
        public String ApplicationName => Configuration.ApplicationName;

        public ActionInfo ActionInfo { get; set; }

        private void BuildActionInfo()
        {
            ActionInfo = new ActionInfo(this);

            if (String.IsNullOrEmpty(Parameters.Action))
            {
                //ShouldGenerateCqrs = false;

                //ActionInfo = ActionInfoItems.NullActionInfo;
                return;
            }

            
            ////ShouldGenerateCqrs = true;

            //ApiRequestBaseClassName = "AertssenRequest";
            //ApiResponseBaseClassName = "AertssenResponse";
            //ActionOperationName = $"{ActionName}{EntityName}";

            //RequiresFactory = false;
            //RequiresDataDto = false;
            //RequiresChangesHandler = false;

            ////IsListAction = false;
            ////IsGetAction = false;
            ////IsDeleteAction = false;
            ////IsStoreAction = false;
            ////IsCreateAction = false;
            ////IsUpdateAction = false;
            ////IsWithoutResponse = false;

            //ShouldGenerateCqrsOutputClass = true;

            //switch (ActionName)
            //{
            //    case "Get":
            //        RequiresDataDto = true;
            //        //CqrsInputType = "Query";
            //        //CqrsOutputType = "ViewModel";
            //        IsGetAction = true;

            //        break;

            //    case "List":
            //        RequiresDataDto = true;
            //        //CqrsInputType = "Query";
            //        //CqrsOutputType = "ViewModel";
            //        IsListAction = true;
            //        ShouldGenerateCqrsOutputClass = false;

            //       ApiRequestBaseClassName = "AertssenListRequest";
            //        ApiResponseBaseClassName = $"AertssenListResponse<{this.DataDtoClass}>";
            //        ActionOperationName = $"{ActionName}{PluralEntityName}";
            //        break;

            //    case "Store":
            //        //CqrsInputType = "Command";
            //        //CqrsOutputType = "Result";
            //        RequiresFactory = true;
            //        RequiresChangesHandler = true;
            //        //RequiresActionDto = true;
            //        IsStoreAction = true;

            //        //IsWithoutResponse = Parameters.WithoutResponse;
            //        //ShouldGenerateCqrsOutputClass = !IsWithoutResponse;
            //        //if (!Parameters.WithoutResponse)
            //        //{
            //        //    RequiresDataDto = true;
            //        //}
            //        break;

            //    case "Create":
            //        //CqrsInputType = "Command";
            //        //CqrsOutputType = "Result";
            //        RequiresFactory = true;
            //        //RequiresActionDto = true;
            //        IsCreateAction = true;

            //        //IsWithoutResponse = Parameters.WithoutResponse;
            //        //ShouldGenerateCqrsOutputClass = !IsWithoutResponse;
            //        //if (!Parameters.WithoutResponse)
            //        //{
            //        //    RequiresDataDto = true;
            //        //}
            //        break;

            //    case "Update":
            //        //CqrsInputType = "Command";
            //        //CqrsOutputType = "Result";
            //        RequiresChangesHandler = true;
            //        //RequiresActionDto = true;
            //        IsUpdateAction = true;

            //        //IsWithoutResponse = Parameters.WithoutResponse;
            //        //ShouldGenerateCqrsOutputClass = !IsWithoutResponse;
            //        //if (!Parameters.WithoutResponse)
            //        //{
            //        //    RequiresDataDto = true;
            //        //}

            //        break;

            //    case "Delete":
            //        //CqrsInputType = "Command";
            //        //CqrsOutputType = "Result";
            //        IsDeleteAction = true;
            //        IsWithoutResponse = true;
            //        ShouldGenerateCqrsOutputClass = !IsWithoutResponse;

            //        break;
            //    default:

            //        break;
            //}

        }
        

        //public String ActionName { get; private set; }
        //public String FullActionName { get; private set; }


        //public string CqrsInputType { get; private set; }
        //public string CqrsOutputType { get; private set; }

        //public Boolean ShouldGenerateCqrs { get; private set; }
        //public Boolean RequiresActionDto { get; private set; }
        public Boolean RequiresDataDto { get; private set; }
        public Boolean RequiresFactory { get; private set; }
        public Boolean RequiresChangesHandler { get; private set; }
        

        //public String ApiRequestBaseClassName { get; private set; }
        //public String ApiResponseBaseClassName { get; private set; }

        //public string ActionOperationName { get; private set; }


        //#region Type Of Action
        //public Boolean IsListAction { get; private set; }
        //public Boolean IsGetAction { get; private set; }
        //public Boolean IsDeleteAction { get; private set; }
        //public Boolean IsStoreAction { get; private set; }
        //public Boolean IsUpdateAction { get; private set; }
        //public Boolean IsCreateAction { get; private set; }

        //public Boolean IsWithoutResponse { get; private set; }
        //#endregion

        #region Filtering of class generation
        public Boolean ShouldGenerateCqrsOutputClass { get; private set; }
        #endregion







        #region DTO
        public string ActionDtoClass => $"{ActionInfo.EntityActionName}Dto";
        public string ActionDtoInterface => ActionDtoClass.ToInterface();

        public string DataDtoClass => $"{EntityName}DataDto";
        public string DataDtoInterface => DataDtoClass.ToInterface();
        #endregion

        #region CQRS
        // Query/Command class
        //public string CqrsInputClassName => $"{ActionEntityName}{CqrsInputType}";
        //public string CqrsOutputClassName => $"{ActionEntityName}{CqrsOutputType}";

        public string GetCqrsOutputClassNameBasedOnAction()
        {
            var outputCqrsOutputClassName = ActionInfo.CqrsOutputClassName;
            if (ActionInfo.IsListAction)
            {
                outputCqrsOutputClassName = $"IQueryable<{DataDtoClass}>";
                return outputCqrsOutputClassName;
            }

            if (!this.ShouldGenerateCqrsOutputClass)
            {
                return "Nothing";
            }

            return outputCqrsOutputClassName;
        }
        public string CqrsHandlerClassName => ActionInfo.CqrsInputClassName.ToHandler();
        public string CqrsValidatorClassName => ActionInfo.CqrsInputClassName.ToValidator();
        public string CqrsFactoryClassName => ActionInfo.CqrsInputClassName.ToFactory();
        public string CqrsChangesHandlerClassName => ActionInfo.CqrsInputClassName.ToChangesHandler();
        #endregion

        #region Api


        public string GetApiOutputClassNameBasedOnAction()
        {
            var outputCqrsOutputClassName = ActionInfo.RequestOutputClassName;
            
            if (!this.ShouldGenerateCqrsOutputClass)
            {
                return "AertssenResponse";
            }

            return outputCqrsOutputClassName;
        }
        #endregion

        #region Services
        public string ServiceContractName => $"{Parameters.PluralEntityName}Service";
        //public string ServiceContractInterfaceName => ServiceContractName.ToInterface();

        public string CqrsServiceContractName => $"{Parameters.PluralEntityName}CqrsService";
        //public string CqrsServiceContractInterfaceName => CqrsServiceContractName.ToInterface();

        public string WebApiServiceContractName => $"WebApi{Parameters.PluralEntityName}Service";

        #endregion

        public string BaseIntegrationTestClass => $"Base{Configuration.ApplicationName}ServiceTest";

        public string BaseCqrsIntegrationTestClass => $"{Configuration.ApplicationName}IntegrationCqrsRequestSpecification";

        public string BaseWebApiIntegrationTestClass => $"{Configuration.ApplicationName}IntegrationApiRequestSpecification";
        


        public String GetHttpMethodAttributeUrlValue()
        {
            if (ActionInfo.IsListAction)
            {
                return $"odata/{PluralEntityName}";
            }

            if (ActionInfo.IsGetAction)
            {
                return $"{PluralEntityName}/"+"{id}";
            }

            if (ActionInfo.IsDeleteAction)
            {
                return $"{PluralEntityName}/" + "{id}";
            }

            if (ActionInfo.IsStoreAction || ActionInfo.IsUpdateAction)
            {
                return $"{PluralEntityName}/" + "{id}";
            }

            if (ActionInfo.IsCreateAction)
            {
                return $"{PluralEntityName}";
            }

            return string.Empty;
        }

        public String GetHttpMethodAttributeMethodValue()
        {
            if (ActionInfo.IsListAction)
            {
                return "Get";
            }

            if (ActionInfo.IsGetAction)
            {
                return $"Get";
            }

            if (ActionInfo.IsDeleteAction)
            {
                return "Delete";
            }

            if (ActionInfo.IsStoreAction || ActionInfo.IsCreateAction)
            {
                return "Post";
            }


            if (ActionInfo.IsUpdateAction)
            {
                return "Put";
            }
            return string.Empty;
        }
    }
}