using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    public interface IExpressionSyntaxBuilder
    {
        ExpressionSyntax Build();
    }
}