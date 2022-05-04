using System;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Roslyn.Builders;

namespace Pdbc.Cli.App.Extensions
{
    public static class TypeDeclarationSyntaxExtensions
    {
        public static PropertyDeclarationSyntax FindPropertyDeclarationSyntaxFor(this TypeDeclarationSyntax typeSyntax, string name)
        {
            foreach (var property in typeSyntax.Members.OfType<PropertyDeclarationSyntax>())
            {
                if (property.Identifier.ValueText == name)
                    return property;
            }

            return null;
        }
        public static FieldDeclarationSyntax FindVariableDeclarationSyntaxFor(this TypeDeclarationSyntax typeSyntax, string name)
        {
            foreach (var variable in typeSyntax.Members.OfType<FieldDeclarationSyntax>())
            {
                //var second = variable.SelectMany(n => n.Declaration.Variables).Where(n => n.Identifier.ValueText == "secondVariable").Single();

                foreach (var v in variable.Declaration.Variables)
                {
                if (v.Identifier.ValueText == name)
                    return variable;
                }
            }

            return null;
        }

        public static bool IsPropertyDefined(this TypeDeclarationSyntax typeSyntax, string name)
        {
            var property = typeSyntax.FindPropertyDeclarationSyntaxFor(name);
            return property != null;
        }
        public static bool IsVariableDefined(this TypeDeclarationSyntax typeSyntax, string name)
        {
            var property = typeSyntax.FindVariableDeclarationSyntaxFor(name);
            return property != null;
        }


        public static MethodDeclarationSyntax FindMethodDeclarationSyntaxFor(this TypeDeclarationSyntax typeSyntax, string name)
        {
            foreach (var method in typeSyntax.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.Identifier.ValueText == name)
                    return method;
            }

            return null;
        }
        public static ConstructorDeclarationSyntax FindConstructorDeclarationSyntaxFor(this TypeDeclarationSyntax typeSyntax, string name)
        {
            foreach (var method in typeSyntax.Members.OfType<ConstructorDeclarationSyntax>())
            {
                // TODO veriyf parameters available..
                return method;
            }

            return null;
        }

        public static bool IsMethodDefined(this TypeDeclarationSyntax typeSyntax, string name)
        {
            var property = typeSyntax.FindMethodDeclarationSyntaxFor(name);
            return property != null;
        }


        public static ClassDeclarationSyntax AddPropertyToClassIfNotExists(this ClassDeclarationSyntax entity, PropertyDeclarationSyntaxBuilder propertyBuilder)
        {
            var identifier = propertyBuilder.GetIdentifier();
            if (!entity.IsPropertyDefined(identifier))
            {
              entity = entity.AppendMember<ClassDeclarationSyntax>(propertyBuilder.Build());
            }

            return entity;
        }
        public static ClassDeclarationSyntax AddMethodToClassIfNotExists(this ClassDeclarationSyntax entity, MethodDeclarationSyntaxBuilder methodBuilder)
        {
            var identifier = methodBuilder.GetIdentifier();
            if (!entity.IsMethodDefined(identifier))
            {
                entity = entity.AppendMember<ClassDeclarationSyntax>(methodBuilder.Build());
            }

            return entity;
        }

        public static bool IsConstructorDefined(this TypeDeclarationSyntax typeSyntax, string name)
        {
            var property = typeSyntax.FindConstructorDeclarationSyntaxFor(name);
            return property != null;
        }

        public static ClassDeclarationSyntax AddVariableToClassIfNotExists(this ClassDeclarationSyntax entity, VariableDeclarationSyntaxBuilder b)
        {
            var identifier = b.GetIdentifier();
            if (!entity.IsVariableDefined(identifier))
            {
                entity = entity.AppendMember<ClassDeclarationSyntax>(b.Build());
            }

            return entity;
        }
        public static ClassDeclarationSyntax AddConstructorToClassIfNotExists(this ClassDeclarationSyntax entity, ConstructorDeclarationSyntaxBuilder constructorBuilder)
        {
            var identifier = constructorBuilder.GetIdentifier();
            if (!entity.IsConstructorDefined(identifier))
            {
                entity = entity.AppendMember<ClassDeclarationSyntax>(constructorBuilder.Build());
            }

            return entity;
        }
        public static T AppendMember<T>(this TypeDeclarationSyntax syntax, MemberDeclarationSyntax memberDeclarationSyntax) where T : TypeDeclarationSyntax
        {
            var members = syntax.Members.Add(memberDeclarationSyntax);
            return (T)syntax.WithMembers(members);
        }



        
    }
}