using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    public class ObjectCreationExpressionSyntaxBuilder : IExpressionSyntaxBuilder
    {
        public string _name;
        public ObjectCreationExpressionSyntaxBuilder AddStatement(string name)
        {
            _name = name;
            return this;
        }

        public List<KeyValuePair<String, string>> assignmentStatements = new List<KeyValuePair<string, string>>();
        public ObjectCreationExpressionSyntaxBuilder AddAssignementStatement(string to, string from)
        {
            assignmentStatements.Add(new KeyValuePair<string, string>(to, from));
            return this;
        }


        public ExpressionSyntax Build()
        {
            var list = new SeparatedSyntaxList<ExpressionSyntax>();
            assignmentStatements.ForEach(p =>
            {
                list.Add(SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(p.Key),
                    SyntaxFactory.IdentifierName(p.Value)));
            });

            ObjectCreationExpressionSyntax expression = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(_name));
            expression = expression
                .WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, list));

            return expression;
        }
    }
}