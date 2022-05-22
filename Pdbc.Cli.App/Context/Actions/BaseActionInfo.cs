using System;

namespace Pdbc.Cli.App.Context.Actions
{
    public class BaseActionInfo : IActionInfo
    {
        public BaseActionInfo(GenerationContext context)
        {
            if (String.IsNullOrEmpty(context.ActionName))
            {
                this.ShouldGenerateCqrs = false;
                return;
            }

            this.ShouldGenerateCqrs = true;

            this.ActionName = context.ActionName;
            this.ActionEntityName = $"{ActionName}{context.EntityName}"; // ex.. GetRoute, StoreAddress, CalculateInvoice
            this.EntityActionName = $"{context.EntityName}{ActionName}"; // ex.RouteGet, AddressStore, InvoiceCalculate
            //this.FullActionName = $"{context.ActionName}{context.PluralEntityName}";
            ActionOperationName = $"{ActionName}{context.EntityName}";
            PublicActionOperationName = $"{ActionName}{context.EntityName}";


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




        }

        public void CalculateValues()
        {
            CqrsInputClassName = $"{ActionEntityName}{CqrsInputType}";
            CqrsOutputClassName = $"{ActionEntityName}{CqrsOutputType}";

            // Default values for override
            ApiResponseClassNameOverride = ApiResponseClassName;
            CqrsOutputClassNameOverride = CqrsOutputClassName;
        }

        public string ActionName { get; set; }
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

        #region Api
        public string ApiRequestClassName { get; set; }
        public string ApiResponseClassName { get; set; }
        #endregion


        public string CqrsOutputClassNameOverride { get; set; }
        public string ApiResponseClassNameOverride { get; set; }



        //public string CqrsOutputClassForAction { get; set; }
        public bool ShouldGenerateCqrs { get; set; }


        public bool RequiresActionDto { get; set; }
        public bool RequiresDataDto { get; set; }
        public bool RequiresFactory { get; set; }
        public bool RequiresChangesHandler { get; set; }


        public bool ShouldGenerateCqrsOutputClass { get; set; }

        public bool IsQueryAction { get; set; }
        public bool IsCommandAction { get; set; }

        public string CqrsInputClassName { get; set; }
        public string CqrsOutputClassName { get; set; }

        public bool IsListAction { get; set; }
        public bool IsGetAction { get; set; }
        public bool IsDeleteAction { get; set; }
        public bool IsStoreAction { get; set; }
        public bool IsUpdateAction { get; set; }
        public bool IsCreateAction { get; set; }
        public bool IsWithoutResponse { get; set; }
    }
}