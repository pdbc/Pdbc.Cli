using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders.ExpressionBuilders
{
    public class ObjectCreationExpressionSyntaxBuilder : IExpressionSyntaxBuilder
    {
        private readonly string _name;
        public ObjectCreationExpressionSyntaxBuilder(String name)
        {
            _name = name;
        }

        private readonly List<KeyValuePair<String, string>> assignmentStatements = new List<KeyValuePair<string, string>>();
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
                list = list.Add(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(p.Key),
                        IdentifierName(p.Value)));
            });


            ObjectCreationExpressionSyntax expression = ObjectCreationExpression(IdentifierName(_name));
            if (list.Count == 1)
            {
                expression = expression.WithInitializer(InitializerExpression(
                    SyntaxKind.ObjectInitializerExpression, 
                        SingletonSeparatedList<ExpressionSyntax>(list.FirstOrDefault())));
                
            }
            else
            {
                expression = expression
                    .WithInitializer(InitializerExpression(SyntaxKind.ObjectInitializerExpression, list));

            }


            return expression;

        }
    }
}