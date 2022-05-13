using System;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Model;

namespace Pdbc.Cli.App.Context
{
    public class GenerationContext
    {
        public StartupParameters Parameters { get; }
        public GenerationConfiguration Configuration { get; }

        public GenerationContext(StartupParameters parameters,
                                 GenerationConfiguration configuration)
        {
            Parameters = parameters;
            Configuration = configuration;

            CalculateActionName();
        }


        public String EntityName => Parameters.EntityName;
        public string PluralEntityName => Parameters.PluralEntityName;

        public String ActionName { get; private set; }
        public String FullActionName { get; private set; }


        public string CqrsInputType { get; private set; }
        public string CqrsOutputType { get; private set; }

        public Boolean ShouldGenerateCqrs { get; private set; }
        public Boolean RequiresActionDto { get; private set; }
        public Boolean RequiresDataDto { get; private set; }
        public Boolean RequiresFactory { get; private set; }
        public Boolean RequiresChangesHandler { get; private set; }
        

        public String ApiRequestBaseClassName { get; private set; }
        public String ApiResponseBaseClassName { get; private set; }

        public string ActionOperationName { get; private set; }


        #region Type Of Action
        public Boolean IsListAction { get; private set; }
        public Boolean IsGetAction { get; private set; }
        public Boolean IsDeleteAction { get; private set; }
        public Boolean IsStoreAction { get; private set; }
        public Boolean IsUpdateAction { get; private set; }
        public Boolean IsCreateAction { get; private set; }

        public Boolean IsWithoutResponse { get; private set; }
        #endregion

        #region Filtering of class generation
        public Boolean ShouldGenerateCqrsOutputClass { get; private set; }
        #endregion

        #region StandarAction Code

        public void CalculateActionName()
        {
            if (String.IsNullOrEmpty(Parameters.Action))
            {
                ActionName = String.Empty;
                ShouldGenerateCqrs = false;
            }

            ShouldGenerateCqrs = true;
            ActionName = Parameters.Action;
            FullActionName = $"{ActionName}{PluralEntityName}";

            ApiRequestBaseClassName = "AertssenRequest";
            ApiResponseBaseClassName = "AertssenResponse";
            ActionOperationName = $"{ActionName}{EntityName}";

            RequiresFactory = false;
            RequiresDataDto = false;
            RequiresChangesHandler = false;

            IsListAction = false;
            IsGetAction = false;
            IsDeleteAction = false;
            IsStoreAction = false;
            IsCreateAction = false;
            IsUpdateAction = false;
            IsWithoutResponse = false;

            ShouldGenerateCqrsOutputClass = true;

            switch (ActionName)
            {
                case "Get":
                    RequiresDataDto = true;
                    CqrsInputType = "Query";
                    CqrsOutputType = "ViewModel";
                    IsGetAction = true;

                    break;

                case "List":
                    RequiresDataDto = true;
                    CqrsInputType = "Query";
                    CqrsOutputType = "ViewModel";
                    IsListAction = true;
                    ShouldGenerateCqrsOutputClass = false;

                    ApiRequestBaseClassName = "AertssenListRequest";
                    ApiResponseBaseClassName = $"AertssenListResponse<{this.DataDtoClass}>";
                    ActionOperationName = $"{ActionName}{PluralEntityName}";
                    break;

                case "Store":
                    CqrsInputType = "Command";
                    CqrsOutputType = "Result";
                    RequiresFactory = true;
                    RequiresChangesHandler = true;
                    RequiresActionDto = true;
                    IsStoreAction = true;

                    IsWithoutResponse = Parameters.WithoutResponse;
                    ShouldGenerateCqrsOutputClass = !IsWithoutResponse;
                    if (!Parameters.WithoutResponse)
                    {
                        RequiresDataDto = true;
                    }
                    break;

                case "Create":
                    CqrsInputType = "Command";
                    CqrsOutputType = "Result";
                    RequiresFactory = true;
                    RequiresActionDto = true;
                    IsCreateAction = true;

                    IsWithoutResponse = Parameters.WithoutResponse;
                    ShouldGenerateCqrsOutputClass = !IsWithoutResponse;
                    if (!Parameters.WithoutResponse)
                    {
                        RequiresDataDto = true;
                    }
                    break;

                case "Update":
                    CqrsInputType = "Command";
                    CqrsOutputType = "Result";
                    RequiresChangesHandler = true;
                    RequiresActionDto = true;
                    IsUpdateAction = true;

                    IsWithoutResponse = Parameters.WithoutResponse;
                    ShouldGenerateCqrsOutputClass = !IsWithoutResponse;
                    if (!Parameters.WithoutResponse)
                    {
                        RequiresDataDto = true;
                    }

                    break;

                case "Delete":
                    CqrsInputType = "Command";
                    CqrsOutputType = "Result";
                    IsDeleteAction = true;
                    IsWithoutResponse = true;
                    ShouldGenerateCqrsOutputClass = !IsWithoutResponse;

                    break;
                default:

                    break;
            }
            
        }

        #endregion





        #region DTO
        public string ActionDtoClass => $"{EntityActionName}Dto";
        public string ActionDtoInterface => ActionDtoClass.ToInterface();

        public string DataDtoClass => $"{EntityName}DataDto";
        public string DataDtoInterface => DataDtoClass.ToInterface();
        #endregion

        #region CQRS
        // Query/Command class
        public string CqrsInputClassName => $"{ActionEntityName}{CqrsInputType}";
        public string CqrsOutputClassName => $"{ActionEntityName}{CqrsOutputType}";

        public string GetCqrsOutputClassNameBasedOnAction()
        {
            var outputCqrsOutputClassName = CqrsOutputClassName;
            if (IsListAction)
            {
                outputCqrsOutputClassName = $"IQueryable<{DataDtoClass}>";
            }

            if (!this.ShouldGenerateCqrsOutputClass)
            {
                return "Nothing";
            }

            return outputCqrsOutputClassName;
        }
        public string CqrsHandlerClassName => CqrsInputClassName.ToHandler();
        public string CqrsValidatorClassName => CqrsInputClassName.ToValidator();
        public string CqrsFactoryClassName => CqrsInputClassName.ToFactory();
        public string CqrsChangesHandlerClassName => CqrsInputClassName.ToChangesHandler();
        #endregion

        #region Api
        public string RequestInputClassName => $"{ActionEntityName}Request";
        public string RequestOutputClassName => $"{ActionEntityName}Response";

        public string GetApiOutputClassNameBasedOnAction()
        {
            var outputCqrsOutputClassName = RequestOutputClassName;
            
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


        private String EntityActionName => $"{Parameters.EntityName}{ActionName}";
        private String ActionEntityName => $"{ActionName}{Parameters.EntityName}";

        //public String ServiceActionName => $"{ActionName}{Parameters.PluralEntityName}";


        #region FullyQualifiedTypes

        public string DbContextName => $"{Configuration.ApplicationName}DbContext";

        #endregion


        public String GetHttpMethodAttributeUrlValue()
        {
            if (IsListAction)
            {
                return $"odata/{PluralEntityName}";
            }

            if (IsGetAction)
            {
                return $"{PluralEntityName}/"+"{id}";
            }

            if (IsDeleteAction)
            {
                return $"{PluralEntityName}/" + "{id}";
            }

            if (IsStoreAction ||IsUpdateAction)
            {
                return $"{PluralEntityName}/" + "{id}";
            }

            if (IsCreateAction)
            {
                return $"{PluralEntityName}";
            }

            return string.Empty;
        }

        public String GetHttpMethodAttributeMethodValue()
        {
            if (IsListAction)
            {
                return "Get";
            }

            if (IsGetAction)
            {
                return $"Get";
            }

            if (IsDeleteAction)
            {
                return "Delete";
            }

            if (IsStoreAction ||IsCreateAction)
            {
                return "Post";
            }


            if (IsUpdateAction)
            {
                return "Put";
            }
            return string.Empty;
        }
    }
}