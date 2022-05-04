using System.Collections.Generic;

namespace Pdbc.Cli.App.Context
{
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