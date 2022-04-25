using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Visitors
{
    public class MyRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var declaration = (TypeDeclarationSyntax)base.VisitClassDeclaration(node);

            return ((ClassDeclarationSyntax)declaration).Update(
                declaration.Attributes,
                Syntax.TokenList(Syntax.Token(SyntaxKind.PublicKeyword), Syntax.Token(SyntaxKind.SealedKeyword)),
                declaration.Keyword,
                declaration.Identifier,
                declaration.TypeParameterListOpt,
                null,
                declaration.ConstraintClauses,
                declaration.OpenBraceToken,
                declaration.Members,
                declaration.CloseBraceToken,
                declaration.SemicolonTokenOpt);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var typeSyntax = node.Type;

            if (node.Identifier.ValueText == "Id")
            {
                typeSyntax = Syntax.IdentifierName("string");
            }

            var newProperty = Syntax.PropertyDeclaration(
                modifiers: Syntax.TokenList(Syntax.Token(SyntaxKind.PublicKeyword)),
                type: typeSyntax,
                identifier: node.Identifier,
                accessorList: Syntax.AccessorList(
                    accessors: Syntax.List(
                        Syntax.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration,
                            semicolonTokenOpt: Syntax.Token(SyntaxKind.SemicolonToken)),
                        Syntax.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration,
                            semicolonTokenOpt: Syntax.Token(SyntaxKind.SemicolonToken))
                    )
                )
            );

            return newProperty;
        }
    }
}