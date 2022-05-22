using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Model.Items;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    public class ConstructorDeclarationSyntaxBuilder
    {
        public String GetIdentifier()
        {
            return _name;
        }


        private String _name;
        public ConstructorDeclarationSyntaxBuilder WithName(String name)
        {
            _name = name;
            return this;
        }

        private SyntaxKind _modifier = SyntaxKind.PublicKeyword;
        public ConstructorDeclarationSyntaxBuilder WithModifier(SyntaxKind modifier)
        {
            _modifier = modifier;
            return this;
        }




        private IList<PropertyItem> _parameters = new List<PropertyItem>();
        public ConstructorDeclarationSyntaxBuilder AddParameter(String type, String name)
        {
            _parameters.Add(new PropertyItem(type, name));
            return this;
        }
        private IList<String> _baseParameters = new List<String>();
        public ConstructorDeclarationSyntaxBuilder AddBaseParameter(String name)
        {
            _baseParameters.Add(name);
            return this;
        }



        private IList<IStatementSyntaxBuilder> _statements = new List<IStatementSyntaxBuilder>();
        public ConstructorDeclarationSyntaxBuilder AddStatement(IStatementSyntaxBuilder statementSyntaxBuilder)
        {
            _statements.Add(statementSyntaxBuilder);
            return this;
        }

        public ConstructorDeclarationSyntaxBuilder AddStatement(string statement)
        {
            var statementSyntaxBuilder = new StatementSyntaxBuilder(statement);
            _statements.Add(statementSyntaxBuilder);
            return this;
        }

        public ConstructorDeclarationSyntax Build()
        {
            var parameterList = ParameterList(SeparatedList<ParameterSyntax>());
            foreach (var p in _parameters)
            {
                parameterList =
                    parameterList.AddParameters(Parameter(Identifier(p.Name))
                        .WithType(SyntaxFactory.ParseTypeName(p.Type)));
            }

            ConstructorDeclarationSyntax method = ConstructorDeclaration(_name)
                .AddModifiers(Token(_modifier))
                .WithParameterList(parameterList);

            var argumentList = ArgumentList(SeparatedList<ArgumentSyntax>());
            foreach (var p in _baseParameters)
            {
                argumentList =
                    argumentList.AddArguments(Argument(IdentifierName(p)));
            }

            if (_baseParameters.Any())
            {
                method = method.WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                    .WithArgumentList(argumentList));

            }

            var syntaxList = new List<StatementSyntax>();
            foreach (var s in _statements)
            {
                var statement = s.Build();
                syntaxList.Add(statement);
            }

            method = method.WithBody(Block(syntaxList));


            return method;
        }
        
    }
}
