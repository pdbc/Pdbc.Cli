using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    internal class InterfaceDeclarationSyntaxBuilder
    {
        private String _namespace;
        public InterfaceDeclarationSyntaxBuilder ForNamespace(String @namespace)
        {
            _namespace = @namespace;
            return this;
        }

        private String _name;
        public InterfaceDeclarationSyntaxBuilder WithName(String name)
        {
            _name = name;
            return this;
        }

        private SyntaxKind _modifier = SyntaxKind.PublicKeyword;
        public InterfaceDeclarationSyntaxBuilder WithModifier(SyntaxKind modifier)
        {
            _modifier = modifier;
            return this;
        }

        public IList<String> _usingStatements = new List<String>();
        public InterfaceDeclarationSyntaxBuilder AddUsingStatement(string statement)
        {
            _usingStatements.Add(statement);
            return this;
        }


        public InterfaceDeclarationSyntaxBuilder AddUsingAertssenFrameworkAuditModel()
        {
            AddUsingStatement("Aertssen.Framework.Audit.Core.Model.Base");
            return this;
        }
        public InterfaceDeclarationSyntaxBuilder AddUsingAertssenFrameworkRepositories()
        {
            AddUsingStatement("Aertssen.Framework.Data.Repositories");
            return this;
        }

        public InterfaceDeclarationSyntaxBuilder AddUnitTestUsingStatement()
        {
            AddUsingStatement("Aertssen.Framework.Tests");
            AddUsingStatement("Aertssen.Framework.Tests.Extensions");
            AddUsingStatement("NUnit.Framework");
            return this;
        }

        public InterfaceDeclarationSyntaxBuilder AddAertssenFrameworkCoreUsingStatements()
        {
            AddUsingStatement("Aertssen.Framework.Core.Builders");
            AddUsingStatement("Aertssen.Framework.Core.Extensions");
            return this;
        }
        public InterfaceDeclarationSyntaxBuilder AddAertssenFrameworkContractUsingStatements()
        {
            AddUsingStatement("Aertssen.Framework.Api.Contracts");
            AddUsingStatement("Aertssen.Framework.Api.Contracts.Attributes");
            return this;
        }


        public IList<String> _baseClasses = new List<String>();
        public InterfaceDeclarationSyntaxBuilder AddBaseClass(string baseClass)
        {
            _baseClasses.Add(baseClass);
            return this;
        }

        private bool _isTestFixture;
        public InterfaceDeclarationSyntaxBuilder AddTestFixtureAttribute(bool isTestFixture)
        {
            _isTestFixture = isTestFixture;
            return this;
        }

        public InterfaceDeclarationSyntax Build()
        {
            var compilationUnitSyntax = SyntaxFactory.CompilationUnit()
                .AddUsingStatements(_usingStatements.ToArray());

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(_namespace));

            var classDeclaration = SyntaxFactory.InterfaceDeclaration(_name)
                    .AddModifiers(SyntaxFactory.Token(_modifier))
                    .AddBaseClasses(_baseClasses.ToArray())
                ;

            //if (_isTestFixture)
            //{
            //    classDeclaration = classDeclaration.AddAttribute("TestFixture");
            //}

            // Add class to namespace
            namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);

            compilationUnitSyntax = compilationUnitSyntax.AddMembers(namespaceDeclaration);

            return compilationUnitSyntax.GetInterfaceDeclarationSyntaxFrom();
        }


    }
}