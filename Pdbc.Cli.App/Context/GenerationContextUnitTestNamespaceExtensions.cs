namespace Pdbc.Cli.App.Context
{
    public static class GenerationContextUnitTestNamespaceExtensions
    {
        public static string GetNamespaceForDomainModelHelpers(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Tests.Helpers.Domain.{context.PluralEntityName}";
            return result;
        }
        
        public static string GetNamespaceForIntegrationTests(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Integration.Tests.IntegrationTests.{context.PluralEntityName}";

            return result;
        }
      
        public static string GetNamespaceForIntegationTestDataExtensions(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.IntegrationTests.Data.Extensions";

            return result;
        }
        public static string GetNamespaceForCoreCqrsTestDataBuilders(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Tests.Helpers.CQRS.{context.PluralEntityName}";

            return result;
        }
        public static string GetNamespaceForApiTestDataBuilders(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Tests.Helpers.Api.{context.PluralEntityName}";

            return result;
        }
        public static string GetNamespaceForDomainTestDataBuilders(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Tests.Helpers.Domain.{context.PluralEntityName}";

            return result;
        }
    }
}