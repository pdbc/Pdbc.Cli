using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    public class PropertyDeclarationSyntaxBuilder
    {
        public PropertyDeclarationSyntaxBuilder()
        {
            
        }

        public PropertyDeclarationSyntaxBuilder(String type, string name)
        {
            this._type = type;
            this._name = name;
        }

        public String GetIdentifier()
        {
            return _name;
        }

        private String _type;
        public PropertyDeclarationSyntaxBuilder ForType(String type)
        {
            _type = type;
            return this;
        }

        private String _name;
        public PropertyDeclarationSyntaxBuilder WithName(String name)
        {
            _name = name;
            return this;
        }

        private SyntaxKind _modifier = SyntaxKind.PublicKeyword;
        public PropertyDeclarationSyntaxBuilder WithModifier(SyntaxKind modifier)
        {
            _modifier = modifier;
            return this;
        }

        private Boolean _includeGetter = true;
        private Boolean _includeSetter = true;

        private String _dependencyType = null;
        public PropertyDeclarationSyntaxBuilder WithDependencyType(String dependencyType)
        {
            _includeGetter = false;
            _includeSetter = false;

            _dependencyType = dependencyType;
            return this;
        }
        public PropertyDeclarationSyntax Build()
        {
            var property = PropertyDeclaration(ParseTypeName(_type), _name)
                    .AddModifiers(Token(_modifier))
                ;

            if (_includeGetter)
            {
                property = property
                    .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            }

            if (_includeSetter)
            {
                property = property
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            }

            if (_dependencyType != null)
            {
                property = property
                    .WithExpressionBody(
                    ArrowExpressionClause(
                        InvocationExpression(
                            GenericName(
                                Identifier("Dependency")
                            ).WithTypeArgumentList(
                                TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(_dependencyType))))
                        )
                    )
                ).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
            }

            return property;
        }
    }
}
