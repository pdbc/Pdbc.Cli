using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pdbc.Cli.App.Extensions;

namespace Pdbc.Cli.App.Context.Actions
{
    public class ActionInfo : IActionInfo
    {
        public string ActionName { get; }
        public String ActionEntityName { get; }
        public String EntityActionName { get; }

        public string ActionOperationName { get; private set; }
        public string PublicActionOperationName { get; private set; }


        #region CQRS & API Base Classes/Interfaces
        public string CqrsInputType { get; private set; }
        public string CqrsOutputType { get; private set; }


        public String ApiRequestBaseClassName { get; private set; }
        public String ApiResponseBaseClassName { get; private set; }
        #endregion

        public string ApiRequestClassName { get; private set; }
        public string ApiResponseClassName { get; private set; }

        public ActionInfo(GenerationContext context)
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

            switch (ActionName)
            {
                case "Get":
                    IsQueryAction = true;
                    RequiresDataDto = true;
                  
                    IsGetAction = true;

                    ShouldGenerateCqrsOutputClass = true;
                    break;

                case "List":
                    IsQueryAction = true;
                    RequiresDataDto = true;

                    IsListAction = true;

                    //ShouldGenerateCqrsOutputClass = false;

                    ApiRequestBaseClassName = "AertssenListRequest";
                    ApiResponseBaseClassName = $"AertssenListResponse<{context.EntityName.ToDataDto()}>";
                    ActionOperationName = $"{ActionName}{context.PluralEntityName}";
                    PublicActionOperationName = $"Get{context.PluralEntityName}";
                    break;

                case "Store":
                    IsCommandAction = true;
                    //RequiresFactory = true;
                    //RequiresChangesHandler = true;
                    RequiresActionDto = true;
                    IsStoreAction = true;

                    //IsWithoutResponse = Parameters.WithoutResponse;
                    //ShouldGenerateCqrsOutputClass = !IsWithoutResponse;
                    //if (!Parameters.WithoutResponse)
                    //{
                    //    RequiresDataDto = true;
                    //}
                    break;

                case "Create":
                    IsCommandAction = true;
                    //RequiresFactory = true;
                    RequiresActionDto = true;
                    IsCreateAction = true;

                    //IsWithoutResponse = Parameters.WithoutResponse;
                    //ShouldGenerateCqrsOutputClass = !IsWithoutResponse;
                    //if (!Parameters.WithoutResponse)
                    //{
                    //    RequiresDataDto = true;
                    //}
                    break;

                case "Update":
                    IsCommandAction = true;
                    //RequiresChangesHandler = true;
                    RequiresActionDto = true;
                    IsUpdateAction = true;

                    //IsWithoutResponse = Parameters.WithoutResponse;
                    //ShouldGenerateCqrsOutputClass = !IsWithoutResponse;
                    //if (!Parameters.WithoutResponse)
                    //{
                    //    RequiresDataDto = true;
                    //}

                    break;

                case "Delete":
                    IsCommandAction = true;

                    IsDeleteAction = true;
                    //IsWithoutResponse = true;
                    //ShouldGenerateCqrsOutputClass = !IsWithoutResponse;

                    break;
                default:

                    break;
            }

            if (IsQueryAction)
            {
                CqrsInputType = "Query";
                CqrsOutputType = "ViewModel";
            } 
            else if (IsCommandAction)
            {
                CqrsInputType = "Command";
                CqrsOutputType = "Result";
            }

            CqrsOutputClassForAction = CqrsOutputClassName;

        }


        
        public Boolean ShouldGenerateCqrs { get; }

        public Boolean RequiresActionDto { get; }
        public Boolean RequiresDataDto { get; }
        public Boolean ShouldGenerateCqrsOutputClass { get; }


        public Boolean IsQueryAction { get; }
        public Boolean IsCommandAction { get; }


        //#region CQRS classes Names
        //public string CqrsInputClassName => $"{ActionEntityName}{CqrsInputType}";
        //public string CqrsOutputClassName => $"{ActionEntityName}{CqrsOutputType}";
        //#endregion
        
        #region Type Of Action
        public Boolean IsListAction { get; private set; }
        public Boolean IsGetAction { get; private set; }
        public Boolean IsDeleteAction { get; private set; }
        public Boolean IsStoreAction { get; private set; }
        public Boolean IsUpdateAction { get; private set; }
        public Boolean IsCreateAction { get; private set; }

        public Boolean IsWithoutResponse { get; private set; }
        #endregion



    }
}
