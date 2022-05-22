namespace Pdbc.Cli.App.Context.Actions
{
    public class DeleteActionInfo : BaseActionInfo
    {
        public DeleteActionInfo(GenerationContext context) : base(context)
        {
            IsCommandAction = true;
            CqrsInputType = "Command";
            CqrsOutputType = "Result";

            IsDeleteAction = true;
            IsWithoutResponse = true;

            CalculateValues();

            ShouldGenerateCqrsOutputClass = false;
            ApiResponseClassNameOverride = "AertssenResponse";
            CqrsOutputClassNameOverride = "Nothing";

        }
    }
}