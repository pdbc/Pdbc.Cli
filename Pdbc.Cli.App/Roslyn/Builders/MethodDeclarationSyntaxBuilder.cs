using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Model.Items;
using Pdbc.Cli.App.Roslyn.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pdbc.Cli.App.Roslyn.Builders
{
    public class MethodDeclarationSyntaxBuilder
    {
        public String GetIdentifier()
        {
            return _name;
        }

        private String _type = "void";
        public MethodDeclarationSyntaxBuilder WithReturnType(String type)
        {
            _type = type;
            return this;
        }

        private String _name;
        public MethodDeclarationSyntaxBuilder WithName(String name)
        {
            _name = name;
            return this;
        }

        private SyntaxKind _modifier = SyntaxKind.PublicKeyword;
        public MethodDeclarationSyntaxBuilder WithModifier(SyntaxKind modifier)
        {
            _modifier = modifier;
            return this;
        }

        private bool _isInterfaceMethod;
        public MethodDeclarationSyntaxBuilder IsInterfaceMethod(bool isInterfaceMethod)
        {
            _isInterfaceMethod = isInterfaceMethod;
            return this;
        }

        private bool _isAbstract;
        public MethodDeclarationSyntaxBuilder IsAbstract(bool isAbstract)
        {
            _isAbstract = isAbstract;
            return this;
        }

        private bool _isOverride;
        public MethodDeclarationSyntaxBuilder IsOverride(bool isOverride)
        {
            _isOverride = isOverride;
            return this;
        }

        private bool _isVirtual;
        public MethodDeclarationSyntaxBuilder IsVirtual(bool isVirtual)
        {
            _isVirtual = isVirtual;
            return this;
        }
        private IList<PropertyItem> _parameters = new List<PropertyItem>();
        public MethodDeclarationSyntaxBuilder AddParameter(String type, String name)
        {
            _parameters.Add(new PropertyItem(type, name));
            return this;
        }




        private bool _isUnitTestMethod;
        public MethodDeclarationSyntaxBuilder AddTestAttribute(bool isTestFixture)
        {
            _isUnitTestMethod = isTestFixture;
            return this;
        }
        public MethodDeclarationSyntax Build()
        {
            var method = MethodDeclaration(ParseTypeName(_type), _name)
                    .AddModifiers(Token(_modifier));

            if (_isAbstract)
            {
                method = method.AddModifiers(Token(SyntaxKind.AbstractKeyword));
            }

            if (_isAsync)
            {
                method = method.AddModifiers(Token(SyntaxKind.AsyncKeyword));
            }
            if (_isOverride)
            {
                method = method.AddModifiers(Token(SyntaxKind.OverrideKeyword));
            }
            if (_isVirtual)
            {
                method = method.AddModifiers(Token(SyntaxKind.VirtualKeyword));
            }

            method = method.AddMethodParameters(_parameters);


            if (_isUnitTestMethod)
            {
                method = method.AddAttribute("Test");
            }

            if (_isAbstract || _isInterfaceMethod)
            {
                method =  method.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                return method;
            }
            else
            {

                var syntaxList = new List<StatementSyntax>();
                foreach (var s in _statements)
                {
                    var statement = s.Build();
                    syntaxList.Add(statement);
                }


                method = method.WithBody(Block(syntaxList));

                //if (_bodyStatements.Count == 0)
                //{
                //    method = method.WithBody(Block());
                //} 
                //else 
                //{
                //    var syntaxStatements = new List<StatementSyntax>();
                //    foreach (var b in _bodyStatements)
                //    {
                //        var s = b.Build();
                //        syntaxStatements.Add(s);
                //    }

                //    method = method.WithBody(Block(syntaxStatements));
                //}
            }
            return method;
        }

        //private IList<StatementSyntaxBuilder> _bodyStatements = new List<StatementSyntaxBuilder>();
        //public MethodDeclarationSyntaxBuilder AddBodyStatement(StatementSyntaxBuilder statementBuilder)
        //{
        //    _bodyStatements.Add(statementBuilder);
        //    return this;
        //}


        private IList<IStatementSyntaxBuilder> _statements = new List<IStatementSyntaxBuilder>();
        public MethodDeclarationSyntaxBuilder AddStatement(IStatementSyntaxBuilder statementSyntaxBuilder)
        {
            _statements.Add(statementSyntaxBuilder);
            return this;
        }



        public MethodDeclarationSyntaxBuilder ReturnsTrue()
        {
            _statements = new List<IStatementSyntaxBuilder>()
            {
                new StatementSyntaxBuilder().ThatReturnsTrue()
            };

            return this;
        }

        public MethodDeclarationSyntaxBuilder ThrowsNewNotImplementedException()
        {
            _statements = new List<IStatementSyntaxBuilder>()
            {
                new StatementSyntaxBuilder().ThatThrowsNotImplementedException()
            };

            return this;
        }

        private bool _isAsync;
        public MethodDeclarationSyntaxBuilder Async()
        {
            _isAsync = true;
            return this;
        }
    }
}
