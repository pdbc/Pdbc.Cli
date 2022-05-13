using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Extensions;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    internal class ClassDeclarationSyntaxBuilder
    {
        private String _namespace;
        public ClassDeclarationSyntaxBuilder ForNamespace(String @namespace)
        {
            _namespace = @namespace;
            return this;
        }

        private String _name;
        public ClassDeclarationSyntaxBuilder WithName(String name)
        {
            _name = name;
            return this;
        }

        private SyntaxKind _modifier = SyntaxKind.PublicKeyword;
        public ClassDeclarationSyntaxBuilder WithModifier(SyntaxKind modifier)
        {
            _modifier = modifier;
            return this;
        }

        public IList<String> _usingStatements = new List<String>()
        {
            "System",
            "System.Linq",
            "System.Threading",
            "System.Threading.Tasks"
        };

        public ClassDeclarationSyntaxBuilder AddUsingStatement(string statement)
        {
            _usingStatements.Add(statement);
            return this;
        }


        public ClassDeclarationSyntaxBuilder AddUsingAertssenFrameworkAuditModel()
        {
            AddUsingStatement("Aertssen.Framework.Audit.Core.Model.Base");
            return this;
        }
        public ClassDeclarationSyntaxBuilder AddUsingAertssenFrameworkCqrsInfra()
        {
            AddUsingStatement("Aertssen.Framework.Infra.CQRS.Base");
            return this;
        }
        public ClassDeclarationSyntaxBuilder AddUsingAertssenFrameworkCqrsServices()
        {
            AddUsingStatement("Aertssen.Framework.Services.Cqrs.Base");
            return this;
        }
        public ClassDeclarationSyntaxBuilder AddUsingAertssenFrameworkServices()
        {
            AddUsingStatement("Aertssen.Framework.Infra.Services");
            return this;
        }
        public ClassDeclarationSyntaxBuilder AddUsingAertssenFrameworkValidationInfra()
        {
            AddUsingStatement("Aertssen.Framework.Infra.Validation");
            AddUsingStatement("Aertssen.Framework.Core.Validation");
            return this;
        }
        public ClassDeclarationSyntaxBuilder AddUsingAertssenFrameworkInfraTests()
        {
            AddUsingStatement("Aertssen.Framework.Tests.Infra");
            return this;
        }
        //
        //"",
        public ClassDeclarationSyntaxBuilder AddUsingAertssenFrameworkConfiguration()
        {
            AddUsingStatement("Aertssen.Framework.Data.Configurations");
            AddUsingStatement("Microsoft.EntityFrameworkCore");
            AddUsingStatement("Microsoft.EntityFrameworkCore.Metadata.Builders");
            return this;
        }
        public ClassDeclarationSyntaxBuilder AddUsingAertssenFrameworkRepositories()
        {
            AddUsingStatement("Aertssen.Framework.Data.Repositories");
            return this;
        }
        public ClassDeclarationSyntaxBuilder AddUnitTestUsingStatement()
        {
            AddUsingStatement("Aertssen.Framework.Tests");
            AddUsingStatement("Aertssen.Framework.Tests.Extensions");
            AddUsingStatement("NUnit.Framework");
            return this;
        }

        public ClassDeclarationSyntaxBuilder AddAertssenFrameworkCoreUsingStatements()
        {
            AddUsingStatement("Aertssen.Framework.Core.Builders");
            AddUsingStatement("Aertssen.Framework.Core.Extensions");
            return this;
        }
        public ClassDeclarationSyntaxBuilder AddAertssenFrameworkContractUsingStatements()
        {
            AddUsingStatement("Aertssen.Framework.Api.Contracts");
            AddUsingStatement("Aertssen.Framework.Api.Contracts.Attributes");
            return this;
        }

        public ClassDeclarationSyntaxBuilder AddAertssenFrameworkServiceAgentsUsingStatements()
        {
            AddUsingStatement("Aertssen.Framework.Api.ServiceAgents");
            return this;
        }


        public IList<String> _baseClasses = new List<String>();
        public ClassDeclarationSyntaxBuilder AddBaseClass(string baseClass)
        {
            _baseClasses.Add(baseClass);
            return this;
        }

        private bool _isAbstract;
        public ClassDeclarationSyntaxBuilder IsAbstract(bool isAbstract)
        {
            _isAbstract = isAbstract;
            return this;
        }

        private bool _isTestFixture;
        public ClassDeclarationSyntaxBuilder AddTestFixtureAttribute(bool isTestFixture)
        {
            _isTestFixture = isTestFixture;
            return this;
        }
        private bool _requiresHttpMethodAttribute;
        private String _route;
        private String _verb;
        public ClassDeclarationSyntaxBuilder AddHttpMethodAttribute(string route, string verb)
        {
            _requiresHttpMethodAttribute = true;
            //[HttpAction("odata/assets", httpMethod: Method.Get)]
            _route = route;
            _verb = verb;
            return this;
        }

        public ClassDeclarationSyntax Build()
        {
            var compilationUnitSyntax = CompilationUnit()
                    .AddUsingStatements(_usingStatements.ToArray());

            var namespaceDeclaration = NamespaceDeclaration(ParseName(_namespace));

            var classDeclaration = ClassDeclaration(_name)
                .AddModifiers(Token(_modifier));


            if (_isAbstract)
            {
                classDeclaration = classDeclaration.AddModifiers(Token(SyntaxKind.AbstractKeyword));
            }

            classDeclaration = classDeclaration.AddBaseClasses(_baseClasses.ToArray());

            if (_isTestFixture)
            {
                classDeclaration = classDeclaration.AddAttribute("TestFixture");
            }
            if (_requiresHttpMethodAttribute)
            {
                var httpMethoAttribute = $"HttpAction(\"{_route}\", Method.{_verb})";
                classDeclaration = classDeclaration.AddAttribute(httpMethoAttribute);
            }
            // Add class to namespace
            namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);

            compilationUnitSyntax = compilationUnitSyntax.AddMembers(namespaceDeclaration);

            return compilationUnitSyntax.GetClassDeclarationSyntaxFrom(); ;
        }


    }
}