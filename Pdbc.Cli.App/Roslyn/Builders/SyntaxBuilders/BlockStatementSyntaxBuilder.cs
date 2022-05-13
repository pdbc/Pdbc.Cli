using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    public class BlockStatementSyntaxBuilder : IStatementSyntaxBuilder
    {
        private IList<IStatementSyntaxBuilder> _statements = new List<IStatementSyntaxBuilder>();
        public BlockStatementSyntaxBuilder AddStatement(IStatementSyntaxBuilder statementSyntaxBuilder)
        {
            _statements.Add(statementSyntaxBuilder);
            return this;
        }

        public StatementSyntax Build()
        {
            var syntaxList = new List<StatementSyntax>();
            foreach (var s in _statements)
            {
                var statement = s.Build();
                syntaxList.Add(statement);
            }

            
            return SyntaxFactory.Block(syntaxList);
        }
    }
}