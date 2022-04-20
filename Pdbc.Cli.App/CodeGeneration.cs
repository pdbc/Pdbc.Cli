using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pdbc.Cli.App
{
    public class CodeGeneration
    {
        /// <summary>
        /// Create a class from scratch.
        /// </summary>
        public String CreateRepository(String @namespaceString, String entityName)
        {
            // Create a namespace: (namespace CodeGenerationSample)
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@namespaceString)).NormalizeWhitespace();

            // Add System using statement: (using System)
            @namespace = @namespace.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Aertssen.Framework.Audit.Core.Model.Base")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Aertssen.Framework.Data.Repositories")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("IM.Scharnier.Domain.Model")));

            //  Create a class: (class Order)
            var interfaceDeclaration = SyntaxFactory.InterfaceDeclaration($"I{entityName}Repository");

            // Add the public modifier: (public class Order)
            interfaceDeclaration = interfaceDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            // Inherit BaseEntity<T> and implement IHaveIdentity: (public class Order : BaseEntity<T>, IHaveIdentity)
            interfaceDeclaration = interfaceDeclaration.AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"IEntityRepository<{entityName}>")));

            // Add the class to the namespace.
            //@namespace = @namespace.AddMembers(interfaceDeclaration);


            //  Create a class: (class Order)
            var classDeclaration = SyntaxFactory.ClassDeclaration($"{entityName}Repository");

            // Add the public modifier: (public class Order)
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            // Inherit BaseEntity<T> and implement IHaveIdentity: (public class Order : BaseEntity<T>, IHaveIdentity)
            classDeclaration = classDeclaration.AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"EntityFrameworkRepository<{entityName}>")),
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"I{entityName}Repository")));


            // Add the class to the namespace.
            @namespace = @namespace.AddMembers(interfaceDeclaration, classDeclaration);

            
            // Normalize and get code as string.
            var interfaceCode = @namespace
                .NormalizeWhitespace()
                .ToFullString();




            return interfaceCode;
        }

        /// <summary>
        /// Create a class from scratch.
        /// </summary>
        public String CreateEntityConfiguration(String @namespaceString, String entityName)
        {
            // Create a namespace: (namespace CodeGenerationSample)
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@namespaceString)).NormalizeWhitespace();

            // Add System using statement: (using System)
            @namespace = @namespace.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore.Metadata.Builders")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Aertssen.Framework.Data.Configurations")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("IM.Scharnier.Domain.Model")));

            //  Create a class: (class Order)
            var classDeclaration = SyntaxFactory.ClassDeclaration($"{entityName}Configuration");

            // Add the public modifier: (public class Order)
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            // Inherit BaseEntity<T> and implement IHaveIdentity: (public class Order : BaseEntity<T>, IHaveIdentity)
            classDeclaration = classDeclaration.AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"AuditableIdentifiableMapping<{entityName}>")));


            // Create a stament with the body of a method.
            var syntaxCode = SyntaxFactory.ParseStatement("base.Configure(builder);");


            var methodDeclarationConfigure = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Configure")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("builder"))
                    .WithType(SyntaxFactory.ParseTypeName($"EntityTypeBuilder<{entityName}>")))
                .WithBody(SyntaxFactory.Block(syntaxCode));

            // Add the field, the property and method to the class.
            classDeclaration = classDeclaration.AddMembers(
                methodDeclarationConfigure);


            // Add the class to the namespace.
            @namespace = @namespace.AddMembers(classDeclaration);


            // Normalize and get code as string.
            var interfaceCode = @namespace
                .NormalizeWhitespace()
                .ToFullString();




            return interfaceCode;
        }

        /// <summary>
        /// Create a class from scratch.
        /// </summary>
        public String CreateEntityClass(String @namespaceString, String entityName)
        {
            // Create a namespace: (namespace CodeGenerationSample)
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(@namespaceString)).NormalizeWhitespace();

            // Add System using statement: (using System)
            @namespace = @namespace.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Aertssen.Framework.Audit.Core.Model.Base")));

            //  Create a class: (class Order)
            var classDeclaration = SyntaxFactory.ClassDeclaration(entityName);

            // Add the public modifier: (public class Order)
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            // Inherit BaseEntity<T> and implement IHaveIdentity: (public class Order : BaseEntity<T>, IHaveIdentity)
            classDeclaration = classDeclaration.AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"BaseEquatableAuditableEntity<{entityName}>")),
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IInterfacingEntity")));

            // Create a Property: (public int Quantity { get; set; })
            var propertyDeclarationExternalSystem = GenerateProperty("String", "ExternalSystem");
            var propertyDeclarationExternalIdentification = GenerateProperty("String", "ExternalIdentification"); 
            var propertyDeclarationDateModified = GenerateProperty("DateTimeOffset", "DateModified"); 

            // Create a stament with the body of a method.
            var syntaxShouldAuditPropertyChanged = SyntaxFactory.ParseStatement("return true;");

            // Create a method
            var methodDeclarationShouldAuditPropertyChanged = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("bool"), "ShouldAuditPropertyChangeFor")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("propertyName"))
                                            .WithType(SyntaxFactory.ParseTypeName("String")))
                .WithBody(SyntaxFactory.Block(syntaxShouldAuditPropertyChanged));

            var syntaxGetAuditProperties = SyntaxFactory.ParseStatement(@"
            return new AuditProperties()
            {
                AreaId = this.Id,
                AreaType = """",
                ObjectId = this.Id,
                ObjectType = this.GetType().Name,
                ObjectInfo = $""""
            };
            ");

            // Create a method
            var methodDeclarationGetAuditProperties = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("IAuditProperties"), "GetAuditProperties")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
                .WithBody(SyntaxFactory.Block(syntaxGetAuditProperties));


            // Add the field, the property and method to the class.
            classDeclaration = classDeclaration.AddMembers(
                propertyDeclarationExternalSystem,
                propertyDeclarationExternalIdentification,
                propertyDeclarationDateModified,

                methodDeclarationShouldAuditPropertyChanged,
                methodDeclarationGetAuditProperties);

            // Add the class to the namespace.
            @namespace = @namespace.AddMembers(classDeclaration);

            // Normalize and get code as string.
            var code = @namespace
                .NormalizeWhitespace()
                .ToFullString();

            return code;
        }

        private static PropertyDeclarationSyntax GenerateProperty(String type, String name)
        {
            var propertyDeclarationExternalSystem = SyntaxFactory
                .PropertyDeclaration(SyntaxFactory.ParseTypeName(type), name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            return propertyDeclarationExternalSystem;
        }
    }
}
