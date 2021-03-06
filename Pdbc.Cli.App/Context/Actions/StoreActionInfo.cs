using System;
using Pdbc.Cli.App.Extensions;

namespace Pdbc.Cli.App.Context.Actions
{
    public class StoreActionInfo : BaseActionInfo
    {
        public StoreActionInfo(GenerationContext context) : base(context)
        {
            //IsQueryAction = false;
            CqrsInputType = "Command";
            CqrsOutputType = "Result";

            ShouldGenerateCqrsOutputClass = context.Parameters.ReturnDataDto;
            //ShouldGenerateCqrsOutputClass = true;

            RequiresDataDto = true;
            RequiresFactory = true;
            RequiresChangesHandler = true;
            RequiresActionDto = true;

            IsStoreAction = true;

            ActionOperationName = $"{context.ActionName}{context.EntityName}";

            CalculateValues();

            HttpMethodAttributeMethod = "Post";
            HttpMethodAttributeUrl = $"{context.PluralEntityName}/" + "{id}";

            if (!context.Parameters.ReturnDataDto)
            {
                base.CqrsOutputClassNameOverride = "Nothing";
                base.ApiResponseClassNameOverride = "AertssenResponse";
            }
        }



    }
}