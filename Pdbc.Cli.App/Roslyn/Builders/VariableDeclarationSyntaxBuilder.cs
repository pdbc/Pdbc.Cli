using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    public class VariableDeclarationSyntaxBuilder
    {
        public String GetIdentifier()
        {
            return _name;
        }

        private String _type;
        public VariableDeclarationSyntaxBuilder ForType(String type)
        {
            _type = type;
            return this;
        }

        private String _name;
        public VariableDeclarationSyntaxBuilder WithName(String name)
        {
            _name = name;
            return this;
        }

        private SyntaxKind _modifier = SyntaxKind.PublicKeyword;
        public VariableDeclarationSyntaxBuilder WithModifier(SyntaxKind modifier)
        {
            _modifier = modifier;
            return this;
        }

        public FieldDeclarationSyntax Build()
        {
            var propertyDeclarationExternalSystem = FieldDeclaration(VariableDeclaration(ParseTypeName(_type))
                    .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(
                        VariableDeclarator(Identifier(_name)))))
                .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                .AddModifiers(Token(SyntaxKind.ReadOnlyKeyword));
            
            return propertyDeclarationExternalSystem;
        }
    }
}
