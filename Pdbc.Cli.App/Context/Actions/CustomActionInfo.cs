using System;
using Pdbc.Cli.App.Extensions;

namespace Pdbc.Cli.App.Context.Actions
{
    public class CustomActionInfo : BaseActionInfo
    {
        public CustomActionInfo(GenerationContext context) : base(context)
        {
            CqrsInputType = "Command";
            CqrsOutputType = "Result";
            
            ShouldGenerateCqrsOutputClass = context.Parameters.ReturnDataDto;

            //RequiresDataDto = true;
            //RequiresActionDto = true;
            

            ActionOperationName = $"{context.ActionName}";

            CalculateValues();

            HttpMethodAttributeMethod = "Post";
            HttpMethodAttributeUrl = $"{context.PluralEntityName}/" + "{id}" + $"/{ActionOperationName}";

            if (!context.Parameters.ReturnDataDto)
            {
                base.CqrsOutputClassNameOverride = "Nothing";
                base.ApiResponseClassNameOverride = "AertssenResponse";
            }
        }



    }
}