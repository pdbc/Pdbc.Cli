using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    public class AssignmentSyntaxBuilder : IStatementSyntaxBuilder
    {
        public AssignmentSyntaxBuilder(String variableName, string parameterName)
        {
            _variableName = variableName;
            _parameterName = parameterName;
        }

        public String _variableName;
        public String _parameterName;

        //public AssignmentSyntaxBuilder WithVariableName(string parameterName)
        //{
        //    _variableName = parameterName;
        //    return this;
        //}


        //public AssignmentSyntaxBuilder WithParameterName(string parameterName)
        //{
        //    _parameterName = parameterName;
        //    return this;
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