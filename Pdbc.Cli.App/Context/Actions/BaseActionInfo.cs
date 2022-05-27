using System;

namespace Pdbc.Cli.App.Context.Actions
{
    public abstract class BaseActionInfo : IActionInfo
    {
        public BaseActionInfo(GenerationContext context)
        {
            if (String.IsNullOrEmpty(context.ActionName))
            {
                this.ShouldGenerateCqrs = false;
                return;
            }

            this.ShouldGenerateCqrs = true;

            //this.ActionName = context.ActionName;
            this.ActionEntityName =
                $"{context.ActionName}{context.EntityName}"; // ex.. GetRoute, StoreAddress, CalculateInvoice
            this.EntityActionName =
                $"{context.EntityName}{context.ActionName}"; // ex.RouteGet, AddressStore, InvoiceCalculate
            //this.FullActionName = $"{context.ActionName}{context.PluralEntityName}";
            ActionOperationName = $"{context.ActionName}{context.EntityName}";
            PublicActionOperationName = $"{context.ActionName}{context.EntityName}";


            ApiRequestBaseClassName = "AertssenRequest";
            ApiResponseBaseClassName = "AertssenResponse";

            ApiRequestClassName = $"{ActionEntityName}Request";
            ApiResponseClassName = $"{ActionEntityName}Response";


            // Reset fields
            IsListAction = false;
            IsGetAction = false;
            IsDeleteAction = false;
            IsStoreAction = false;
            IsCreateAction = false;
            IsUpdateAction = false;

            IsWithoutResponse = false;
            ShouldGenerateCqrsOutputClass = false;

            ServiceContractName = $"{context.PluralEntityName}Service";

            //CqrsServiceContractName = $"{context.PluralEntityName}CqrsService";

        }

        public void CalculateValues()
        {
            CqrsInputClassName = $"{ActionEntityName}{CqrsInputType}";
            CqrsOutputClassName = $"{ActionEntityName}{CqrsOutputType}";

            // Default values for override
            ApiResponseClassNameOverride = ApiResponseClassName;
            CqrsOutputClassNameOverride = CqrsOutputClassName;
        }



        //public string ActionName { get; set; }
        public string ActionEntityName { get; set; }
        public string EntityActionName { get; set; }

        public string ActionOperationName { get; set; }
        public string PublicActionOperationName { get; set; }



        #region Cqrs & API Base classes
        public string CqrsInputType { get; set; }
        public string CqrsOutputType { get; set; }
        public string ApiRequestBaseClassName { get; set; }
        public string ApiResponseBaseClassName { get; set; }
        #endregion

        #region Cqrs/Api/Service names

        public string CqrsInputClassName { get; set; }
        public string CqrsOutputClassName { get; set; }

        public string ApiRequestClassName { get; set; }
        public string ApiResponseClassName { get; set; }

        public string ServiceContractName { get; set; }

        #endregion


        public string CqrsOutputClassNameOverride { get; set; }
        public string ApiResponseClassNameOverride { get; set; }

        public bool ShouldGenerateCqrs { get; set; }


        public bool RequiresActionDto { get; set; }
        public bool RequiresDataDto { get; set; }
        public bool RequiresFactory { get; set; }
        public bool RequiresChangesHandler { get; set; }


        public bool ShouldGenerateCqrsOutputClass { get; set; }

        //public bool IsQueryAction { get; set; }
        //public bool IsCommandAction { get; set; }



        public bool IsListAction { get; set; }
        public bool IsGetAction { get; set; }
        public bool IsDeleteAction { get; set; }
        public bool IsStoreAction { get; set; }
        public bool IsUpdateAction { get; set; }
        public bool IsCreateAction { get; set; }
        public bool IsWithoutResponse { get; set; }


        //public String UrlPostfix { get; set; }
        public String HttpMethodAttributeUrl { get; set; }

        public String HttpMethodAttributeMethod { get; set; }


        //public String GetHttpMethodAttributeMethodValue()
        //{

        //    if (ActionInfo.IsStoreAction || ActionInfo.IsCreateAction)
        //    {
        //        return "";
        //    }


        //    if (ActionInfo.IsUpdateAction)
        //    {
        //        return "Put";
        //    }
        //    return string.Empty;
        //}
        //public String GetHttpMethodAttributeUrlValue()
        //{
        //    if (ActionInfo.IsStoreAction || ActionInfo.IsUpdateAction)
        //    {
        //        return;
        //    }

        //    if (ActionInfo.IsCreateAction)
        //    {
        //        return $"{PluralEntityName}";
        //    }

        //    return string.Empty;
        //}
    }
}