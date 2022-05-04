using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    public class StatementSyntaxBuilder : IStatementSyntaxBuilder
    {
        public string _body;
        public StatementSyntaxBuilder AddStatement(string body)
        {
            _body = body;
            return this;
        }

        public StatementSyntaxBuilder ThatThrowsNotImplementedException()
        {
            _body = "throw new NotImplementedException();";
            return this;
        }
        public StatementSyntaxBuilder ThatReturnsTrue()
        {
            _body = "return true;";
            return this;
        }
        public StatementSyntaxBuilder ThatReturnsAndObject(string objectToCreate)
        {
            _body = $"return new {objectToCreate}();";
            return this;
        }
        public IList<StatementSyntaxBuilder> _subStatements = new List<StatementSyntaxBuilder>();
        public StatementSyntaxBuilder AddSubStatement(StatementSyntaxBuilder statement)
        {
            _subStatements.Add(statement);
            return this;
        }

        public StatementSyntax Build()
        {
            var methodBody = ParseStatement(_body);

            var syntaxStatements = new List<StatementSyntax>();
            if (_subStatements.Any())
            {
                foreach (var b in _subStatements)
                {
                    var s = b.Build();
                    syntaxStatements.Add(s);
                }



                //methodBody = methodBody.ReplaceNode(methodBody.) Block(syntaxStatements);
            }

            return methodBody;

        }
    }
}