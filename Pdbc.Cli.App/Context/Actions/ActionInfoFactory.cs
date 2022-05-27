using System;

namespace Pdbc.Cli.App.Context.Actions
{
    public class ActionInfoFactory
    {
        public IActionInfo BuildActionInfoFor(GenerationContext context)
        {
            switch (context.ActionName)
            {
                case "Get":
                    return new GetActionInfo(context);

                case "List":
                    return new ListActionInfo(context);

                case "Store":
                    return new StoreActionInfo(context);
                    
                case "Create":

                    break;

                case "Update":


                    break;

                case "Delete":
                    return new DeleteActionInfo(context);
                    
                default:

                    
                    break;
            }

            throw new NotImplementedException("No action foresee");
        }
    }
}