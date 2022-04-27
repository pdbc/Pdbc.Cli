﻿using System;
using System.Collections.Generic;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Model;

namespace Pdbc.Cli.App.Context
{
    public class GenerationContext
    {
        public StartupParameters Parameters { get; }
        public GenerationConfiguration Configuration { get; }

        public StandardAction StandardActionInfo { get; private set; }


        public GenerationContext(StartupParameters parameters,
                                 GenerationConfiguration configuration)
        {
            Parameters = parameters;
            Configuration = configuration;

            StandardActionInfo = new StandardAction(Parameters);
            ActionName = StandardActionInfo.CalculateActionName();
        }

        public String EntityName => Parameters.EntityName;
        public string PluralEntityName => Parameters.PluralEntityName;

        public String ActionName { get; private set; }
        public string CqrsInputType => StandardActionInfo.GetCqrsInputType();
        public string CqrsOutputType => StandardActionInfo.GetCqrsOutputType();

        #region DTO
        public string ActionDtoClass =>  $"{EntityActionName}Dto";
        public string ActionDtoInterface => ActionDtoClass.ToInterface();

        public string DataDtoClass => $"{EntityName}DataDto";
        public string DataDtoInterface => DataDtoClass.ToInterface();
        #endregion

        #region CQRS
        // Query/Command class
        public string CqrsInputClassName => $"{EntityActionName}{CqrsInputType}";
        public string CqrsOutputClassName => $"{EntityActionName}{CqrsOutputType}";
        
        public string CqrsHandlerClassName => CqrsInputClassName.ToHandler();
        public string CqrsValidatorClassName => CqrsInputClassName.ToValidator();
        public string CqrsFactoryClassName => CqrsInputClassName.ToFactory();
        public string CqrsChangesHandlerClassName => CqrsInputClassName.ToChangesHandler();
        #endregion

        #region Api
        public string RequestInputClassName => $"{EntityActionName}Request";
        public string RequestOutputClassName => $"{EntityActionName}Response";
        #endregion

        #region Services
        public string ServiceContractName => $"{Parameters.PluralEntityName}Service";
        public string ServiceContractInterfaceName => ServiceContractName.ToInterface();

        public string CqrsServiceContractName => $"{Parameters.PluralEntityName}CqrsService";
        public string CqrsServiceContractInterfaceName => CqrsServiceContractName.ToInterface();

        public string WebApiServiceContractName => $"WebApi{Parameters.PluralEntityName}Service";

        #endregion

        private String EntityActionName => $"{Parameters.EntityName}{ActionName}";
        private String ActionEntityName => $"{ActionName}{Parameters.EntityName}";

        public String ServiceActionName => $"{ActionName}{Parameters.PluralEntityName}";


        #region FullyQualifiedTypes

        public string FactoryType => $"IFactory<{ActionDtoInterface},{EntityName}>";
        public string ChangesHandlerType => $"IChangesHandler<{ActionDtoInterface},{EntityName}>";
        public string RepositoryGenericType => $"IEntityRepository<{EntityName}>";
        //{
        //    return $"";
        //}
        //public static string ToChangesHandlerType(this StandardAction standardAction)
        //{
        //    return $"IChangesHandler<{standardAction.ToActionDtoInterface()},{ standardAction._entityName}>";
        //}

        //public static String ToRepositoryType(this StandardAction standardAction)
        //{
        //    return $"IEntityRepository<{standardAction._entityName}>";
        //}

        #endregion
    }

    public static class UsingStatementExtensions
    {
        public static string[] AddUnitTestUsingStatement(this string[] usings)
        {
            var result = new List<string>(usings);
            result.Add("Aertssen.Framework.Tests");
            result.Add("Aertssen.Framework.Tests.Extensions");
            result.Add("NUnit.Framework");
            return result.ToArray();
        }

        public static string[] AddAertssenFrameworkAuditModelStatements(this string[] usings)
        {
            var result = new List<string>(usings);
            result.Add("Aertssen.Framework.Audit.Core.Model.Base");
            return result.ToArray();
        }

        public static string[] AddAertssenFrameworkCoreUsingStatements(this string[] usings)
        {
            var result = new List<string>(usings);
            result.Add("Aertssen.Framework.Core.Builders");
            result.Add("Aertssen.Framework.Core.Extensions");
            return result.ToArray();
        }
        public static string[] AddAertssenFrameworkContractUsingStatements(this string[] usings)
        {
            var result = new List<string>(usings);
            result.Add("Aertssen.Framework.Api.Contracts");
            result.Add("Aertssen.Framework.Api.Contracts.Attributes");
            return result.ToArray();
        }
        
    }
}