namespace Pdbc.Cli.App.Context.Actions
{
    public class GetActionInfo : BaseActionInfo
    {
        public GetActionInfo(GenerationContext context) : base(context)
        {
            //IsQueryAction = true;
            CqrsInputType = "Query";
            CqrsOutputType = "ViewModel";

            RequiresDataDto = true;

            IsGetAction = true;

            ShouldGenerateCqrsOutputClass = true;
            CalculateValues();


            HttpMethodAttributeMethod = $"Get";
            HttpMethodAttributeUrl = $"{context.PluralEntityName}/" + "{id}";
        }

    }
}