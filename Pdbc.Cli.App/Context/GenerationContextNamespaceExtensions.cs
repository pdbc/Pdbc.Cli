namespace Pdbc.Cli.App.Context
{
    public static class GenerationContextNamespaceExtensions
    {



        public static string GetNamespaceForDomainModelValidations(this GenerationContext context)
        {

            var result = $"{context.RootNamespace}.Domain.Validations";

            return result;
        }
        public static string GetNamespaceForDomainModel(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Domain.Model";

            return result;
        }

        public static string GetNamespaceForData(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Data";

            return result;
        }
        public static string GetNamespaceForCqrsServices(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Services.Cqrs.Services.{context.PluralEntityName}";

            return result;
        }

        public static string GeNameForServiceAgents(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Api.ServiceAgent.{context.PluralEntityName}";

            return result;
        }

        //Locations.Api.Contracts.Services.Routes
        public static string GetNamespaceForDataRepositories(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Data.Repositories";

            return result;
        }

        public static string GetNamespaceForDto(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Dto.{context.PluralEntityName}";

            return result;
        }
        public static string GetNamespaceForDtoTestDataBuilders(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Tests.Helpers.DTO.{context.PluralEntityName}";

            return result;
        }
        public static string GetNamespaceForRequests(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Api.Contracts.Requests.{context.PluralEntityName}";

            return result;
        }
        //Locations.Api.Contracts.Requests.Routes
        public static string GetNamespaceForServices(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Api.Contracts.Services.{context.PluralEntityName}";

            return result;
        }

        public static string GetNamespaceForCoreCqrs(this GenerationContext context)
        {
            var result = $"{context.RootNamespace}.Core.CQRS.{context.PluralEntityName}.{context.ActionName}";

            return result;
        }
    }
}