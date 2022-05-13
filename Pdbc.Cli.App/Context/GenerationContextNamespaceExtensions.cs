﻿namespace Pdbc.Cli.App.Context
{
    public static class GenerationContextNamespaceExtensions
    {
        public static string GetNamespaceForDomainModelHelpers(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Tests.Helpers.Domain.{context.PluralEntityName}";
            return result;
        }

        public static string GetNamespaceForDomainModel(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Domain.Model";

            return result;
        }

        public static string GetNamespaceForData(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Data";

            return result;
        }
        public static string GetNamespaceForIntegrationTests(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Integration.Tests.IntegrationTests.{context.PluralEntityName}";

            return result;
        }
        public static string GetNamespaceForCqrsServices(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Services.Cqrs.Services.{context.PluralEntityName}";

            return result;
        }
        //GetNamespaceForIntegrationTests

        public static string GeNameForServiceAgents(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Api.ServiceAgent.{context.PluralEntityName}";

            return result;
        }
        
        //Locations.Api.Contracts.Services.Routes
        public static string GetNamespaceForDataRepositories(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Data.Repositories";

            return result;
        }

        public static string GetNamespaceForIntegationTestDataExtensions(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.IntegrationTests.Data.Extensions";

            return result;
        }

        public static string GetNamespaceForDto(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Dto.{context.PluralEntityName}";

            return result;
        }


        public static string GetNamespaceForRequests(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Api.Contracts.Requests.{context.PluralEntityName}";

            return result;
        }
        //Locations.Api.Contracts.Requests.Routes
        public static string GetNamespaceForServices(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Api.Contracts.Services.{context.PluralEntityName}";

            return result;
        }

        public static string GetNamespaceForCoreCqrs(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Core.CQRS.{context.PluralEntityName}.{context.ActionName}";

            return result;
        }

        public static string GetNamespaceForCoreCqrsTestDataBuilders(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Tests.Helpers.CQRS.{context.PluralEntityName}";

            return result;
        }
        public static string GetNamespaceForApiTestDataBuilders(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Tests.Helpers.Api.{context.PluralEntityName}";

            return result;
        }
        public static string GetNamespaceForDomainTestDataBuilders(this GenerationContext context)
        {
            var result = $"{context.Configuration.RootNamespace}.Tests.Helpers.Domain.{context.PluralEntityName}";

            return result;
        }
}
}