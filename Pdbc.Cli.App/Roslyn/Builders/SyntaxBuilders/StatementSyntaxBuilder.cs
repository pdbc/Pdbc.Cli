using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders
{
    public class StatementSyntaxBuilder : IStatementSyntaxBuilder
    {
        public StatementSyntaxBuilder()
        {
            
        }

        private  string _body;
        public StatementSyntaxBuilder(string statement)
        {
            _body = statement;
            //if (!statement.Trim().EndsWith(";"))
            //    _body = $"{_body};";
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
        //public IList<StatementSyntaxBuilder> _subStatements = new List<StatementSyntaxBuilder>();
        //public StatementSyntaxBuilder AddSubStatement(StatementSyntaxBuilder statement)
        //{
        //    _subStatements.Add(statement);
        //    return this;
        //}

        public StatementSyntax Build()
        {
            var methodBody = ParseStatement(_body);

            //var syntaxStatements = new List<StatementSyntax>();
            //if (_subStatements.Any())
            //{
            //    foreach (var b in _subStatements)
            //    {
            //        var s = b.Build();
            //        syntaxStatements.Add(s);
            //    }



            //    //methodBody = methodBody.ReplaceNode(methodBody.) Block(syntaxStatements);
            //}

            return methodBody;

        }
    }
}