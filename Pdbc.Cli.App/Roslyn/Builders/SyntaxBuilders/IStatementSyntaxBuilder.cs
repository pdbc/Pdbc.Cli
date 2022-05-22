using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders
{
    public interface IStatementSyntaxBuilder
    {
        StatementSyntax Build();
    }
}