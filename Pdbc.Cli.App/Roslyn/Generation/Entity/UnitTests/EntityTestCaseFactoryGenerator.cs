using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;

namespace Pdbc.Cli.App.Roslyn.Generation.Entity
{
    public static class EntityTestCaseFactoryGenerator
    {
        public static async Task AppendFactoryMethodForDomainEntityToTestCaseFactory(this GenerationService service)
        {
            var className = "TestCaseFactory";
            var subfolders = new[] { "Domain", service.GenerationContext.PluralEntityName };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Tests.Helpers");

            
            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                throw new InvalidOperationException("Cannot append generate entity to non-existing (TestCaseFactory) class");
            }

            
            var fullFilename = entity.SyntaxTree.FilePath;

            entity = await service.AppendUsingStatement(entity, service.GenerationContext.GetNamespaceForDomainModel(), fullFilename);
            entity = await service.AppendUsingStatement(entity, service.GenerationContext.GetNamespaceForDomainModelHelpers(), fullFilename);
            entity = await service.AppendUsingStatement(entity, "System", fullFilename);
            

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName($"Build{service.GenerationContext.EntityName}")
                    .WithReturnType(service.GenerationContext.EntityName)
                    .AddParameter($"Action<{service.GenerationContext.EntityName.ToBuilder()}>","action = null")
                    .AddStatement($"var builder = new {service.GenerationContext.EntityName.ToTestDataBuilder()}();")
                    .AddStatement("action?.Invoke(builder);")
                    .AddStatement("var item = builder.Build();")
                    .AddStatement("return item;")
                , fullFilename);
            // TODO Implement
            //entity = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
            //    .WithName(service.GenerationContext.PluralEntityName)
            //    .ForType($"DbSet<{service.GenerationContext.EntityName}>"), fullFilename);

        }

        public static async Task AppendFactoryMethodForDomainEntityToTestCaseService(this GenerationService service)
        {
            var className = "TestCaseService";
            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Integration.Tests");


            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                throw new InvalidOperationException("Cannot append generate entity to non-existing (TestCaseService) class");
            }


            var fullFilename = entity.SyntaxTree.FilePath;

            entity = await service.AppendUsingStatement(entity, service.GenerationContext.GetNamespaceForDomainModel(), fullFilename);
            entity = await service.AppendUsingStatement(entity, service.GenerationContext.GetNamespaceForDomainModelHelpers(), fullFilename);
            entity = await service.AppendUsingStatement(entity, "System", fullFilename);

            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName($"Setup{service.GenerationContext.EntityName}")
                    .WithReturnType(service.GenerationContext.EntityName)
                    .AddParameter($"Action<{service.GenerationContext.EntityName.ToBuilder()}>", "action = null")
                    .AddStatement($"var item = _factory.Build{service.GenerationContext.EntityName}(action);")
                    .AddStatement($"_context.Set<{service.GenerationContext.EntityName}>().Add(item);")
                    .AddStatement("_context.SaveChanges();")
                    .AddStatement("return item;")
                , fullFilename);

        }

        public static async Task AppendTestDataObjectsMethods(this GenerationService service)
        {
            var className = service.GenerationContext.ApplicationName.ToTestDataObjects();
            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Integration.Tests");


            var entity = await roslynProjectContext.GetClassByName(className);
            if (entity == null)
            {
                throw new InvalidOperationException($"Cannot append generate entity to non-existing ({className}) class");
            }
            var fullFilename = entity.SyntaxTree.FilePath;

            
            entity = await service.AppendUsingStatement(entity, service.GenerationContext.GetNamespaceForDomainModel(), fullFilename);
            entity = await service.AppendUsingStatement(entity, service.GenerationContext.GetNamespaceForDomainModelHelpers(), fullFilename);
            
            var setupMethodName = $"Setup{service.GenerationContext.PluralEntityName}";
            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName(setupMethodName)
                , fullFilename);

            var loadMethodName = $"Load{service.GenerationContext.PluralEntityName}";
            entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                    .WithName(loadMethodName)
                    .AddStatement($"{setupMethodName}();")
                , fullFilename);

            var loadObjectsMethod = entity.FindMethodDeclarationSyntaxFor("LoadObjects");
            if (loadObjectsMethod == null)
            {
                throw new InvalidOperationException($"Cannot append find 'LoadObjects' method");
            }

            var found = false;
            foreach (StatementSyntax n in loadObjectsMethod.Body.Statements)
            {
                var s = n.ToFullString();
                if (s.Contains(loadMethodName))
                {
                    found = true;
                    break;
                }
            }

            // TODO append this method
            if (!found)
            {
                var builder = new StatementSyntaxBuilder($"{loadMethodName}();");
                var s = builder.Build();

                var newMethod = loadObjectsMethod.AddBodyStatements(s);
                var updatedEntity = entity.ReplaceNode(loadObjectsMethod, newMethod);
                await service.SaveAndUpdate(entity, updatedEntity, fullFilename);
            }
                //loadObjectsMethod.Body.Statements
            // Generate Method Setup{PluralEntityName}
            


        }

    }

}