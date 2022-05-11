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

                    if (!Parameters.WithoutResponse)
                    {
                        RequiresDataDto = true;
                    }

                    break;

                case "Delete":

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
        public string CqrsInputClassName => $"{EntityActionName}{CqrsInputType}";
        public string CqrsOutputClassName => $"{EntityActionName}{CqrsOutputType}";

        public string GetCqrsOutputClassNameBasedOnAction()
        {
            var outputCqrsOutputClassName = CqrsOutputClassName;
            if (IsListAction)
            {
                outputCqrsOutputClassName = $"IQueryable<{DataDtoClass}>";
            }

            return outputCqrsOutputClassName;
        }
        public string CqrsHandlerClassName => CqrsInputClassName.ToHandler();
        public string CqrsValidatorClassName => CqrsInputClassName.ToValidator();
        public string CqrsFactoryClassName => CqrsInputClassName.ToFactory();
        public string CqrsChangesHandlerClassName => CqrsInputClassName.ToChangesHandler();
        #endregion

        #region Api
        public string RequestInputClassName => $"{EntityActionName}Request";
        public string RequestOutputClassName => $"{EntityActionName}Response";
        #endregion

        #region Services
        public string ServiceContractName => $"{Parameters.PluralEntityName}Service";
        //public string ServiceContractInterfaceName => ServiceContractName.ToInterface();

        public string CqrsServiceContractName => $"{Parameters.PluralEntityName}CqrsService";
        //public string CqrsServiceContractInterfaceName => CqrsServiceContractName.ToInterface();

        public string WebApiServiceContractName => $"WebApi{Parameters.PluralEntityName}Service";

        #endregion

        private String EntityActionName => $"{Parameters.EntityName}{ActionName}";
        private String ActionEntityName => $"{ActionName}{Parameters.EntityName}";

        public String ServiceActionName => $"{ActionName}{Parameters.PluralEntityName}";


        #region FullyQualifiedTypes

        public string DbContextName => $"{Configuration.ApplicationName}DbContext";

        #endregion


        public String GetHttpMethodAttributeValue()
        {
            if (IsListAction)
            {
                return $"odata/{PluralEntityName}";
            }

            if (IsGetAction)
            {
                return $"{PluralEntityName}/"+"{id}";
            }

            return string.Empty;
        }
    }
}