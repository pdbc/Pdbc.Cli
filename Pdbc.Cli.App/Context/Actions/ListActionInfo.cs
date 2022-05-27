using Pdbc.Cli.App.Extensions;

namespace Pdbc.Cli.App.Context.Actions
{
    public class ListActionInfo : BaseActionInfo
    {
        public ListActionInfo(GenerationContext context) : base(context)
        {
            //IsQueryAction = true;
            CqrsInputType = "Query";
            CqrsOutputType = "ViewModel";

            RequiresDataDto = true;

            IsListAction = true;
            
            ApiRequestBaseClassName = "AertssenListRequest";
            ApiResponseBaseClassName = $"AertssenListResponse<{context.EntityName.ToDataDto()}>";

            ActionOperationName = $"{context.ActionName}{context.PluralEntityName}";
            PublicActionOperationName = $"Get{context.PluralEntityName}";

            CalculateValues();

            CqrsOutputClassNameOverride = $"IQueryable<{context.EntityName.ToDataDto()}>";
            //ApiResponseClassNameOverride = "AertssenResponse";
            //CqrsOutputClassNameOverride = "Nothing";

            HttpMethodAttributeMethod = "Get";
            HttpMethodAttributeUrl = $"odata/{context.PluralEntityName}";

        }


    }
}