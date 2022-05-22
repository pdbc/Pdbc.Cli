using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders
{
    public class AssignmentSyntaxBuilder : IStatementSyntaxBuilder
    {
        public AssignmentSyntaxBuilder(String variableName, string parameterName)
        {
            _variableName = variableName;
            _parameterName = parameterName;
        }

        private readonly String _variableName;
        private readonly String _parameterName;

        public StatementSyntax Build()
        {
            return ExpressionStatement(
                        AssignmentExpression
                        (
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(_variableName),
                            IdentifierName(_parameterName)
                        )
                );
        }
    }
}