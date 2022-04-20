using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App.Visitors
{
    class ClassVirtualizationVisitor : CSharpSyntaxRewriter
    {
        public ClassVirtualizationVisitor()
        {
            Classes = new List<ClassDeclarationSyntax>();
        }

        public List<ClassDeclarationSyntax> Classes { get; set; }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
            Classes.Add(node); // save your visited classes
            return node;
        }
    }
}