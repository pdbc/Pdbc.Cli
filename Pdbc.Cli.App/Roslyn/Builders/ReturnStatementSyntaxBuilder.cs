using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    public class ReturnStatementSyntaxBuilder : IStatementSyntaxBuilder
    {
        //public IList<IExpressionSyntaxBuilder> _subStatements = new List<IExpressionSyntaxBuilder>();
        //public ReturnStatementSyntaxBuilder AddSubStatement(IExpressionSyntaxBuilder statement)
        //{
        //    _subStatements.Add(statement);
        //    return this;
        //}

        //public IExpressionSyntaxBuilder _ExpressionSyntaxBuilder;
        //public ReturnStatementSyntaxBuilder WithExpressionSyntax(IExpressionSyntaxBuilder statement)
        //{
        //    _ExpressionSyntaxBuilder = statement;
        //    return this;
        //}

        private string _name;

        public ReturnStatementSyntaxBuilder WithName(string name)
        {
            _name = name;
            return this;
        }


        public StatementSyntax Build()
        {
            return ReturnStatement
            (
                IdentifierName(_name)
            );
            

        }
    }
}