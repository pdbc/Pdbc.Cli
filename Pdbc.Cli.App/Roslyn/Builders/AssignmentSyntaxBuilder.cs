using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    public class AssignmentSyntaxBuilder : IStatementSyntaxBuilder
    {
        public String _variableName;
        public AssignmentSyntaxBuilder WithVariableName(string parameterName)
        {
            _variableName = parameterName;
            return this;
        }

        public String _parameterName;
        public AssignmentSyntaxBuilder WithParameterName(string parameterName)
        {
            _parameterName = parameterName;
            return this;
        }

        //public ExpressionSyntax Build()
        //{
        //    return SyntaxFactory.AssignmentExpression
        //    (
        //        SyntaxKind.SimpleAssignmentExpression,
        //        SyntaxFactory.IdentifierName(_variableName),
        //        SyntaxFactory.IdentifierName(_parameterName)
        //    );
        //}

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