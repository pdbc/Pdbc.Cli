using Microsoft.CodeAnalysis;

namespace Pdbc.Cli.App.Roslyn.Extensions
{
    public static class SyntaxNodeExtensions
    {
        public static T GetParentNodeOfType<T>(this SyntaxNode? syntaxNode)
            where T : SyntaxNode
        {
            if (syntaxNode == null)
            {
                return null;
            }

            try
            {
                syntaxNode = syntaxNode.Parent;

                if (syntaxNode == null)
                {
                    return null;
                }

                if (syntaxNode.GetType() == typeof(T))
                {
                    return syntaxNode as T;
                }

                return syntaxNode.GetParentNodeOfType<T>();
            }
            catch
            {
                return null;
            }
        }
        
    }
}