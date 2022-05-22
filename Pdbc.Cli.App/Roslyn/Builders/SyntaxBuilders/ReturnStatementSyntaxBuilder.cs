using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Roslyn.Builders.ExpressionBuilders;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders
{
    public class ReturnStatementSyntaxBuilder : IStatementSyntaxBuilder
    {

        private string _name;

        public ReturnStatementSyntaxBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        private ObjectCreationExpressionSyntaxBuilder _objectCreationExpressionSyntaxBuilder;
        public ReturnStatementSyntaxBuilder WithObjectCreationExpression(ObjectCreationExpressionSyntaxBuilder objectCreationExpressionSyntaxBuilder)
        {
            _objectCreationExpressionSyntaxBuilder = objectCreationExpressionSyntaxBuilder;
            return this;
        }

        public StatementSyntax Build()
        {
            if (_name != null)
            {
                return ReturnStatement
                (
                    IdentifierName(_name)
                );
            }


            return ReturnStatement(_objectCreationExpressionSyntaxBuilder.Build());





        }
    }
}