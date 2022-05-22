using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Roslyn.Builders.ExpressionBuilders
{
    public interface IExpressionSyntaxBuilder
    {
        ExpressionSyntax Build();
    }
}