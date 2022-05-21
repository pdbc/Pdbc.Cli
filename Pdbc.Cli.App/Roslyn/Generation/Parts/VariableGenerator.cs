using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;

namespace Pdbc.Cli.App.Roslyn.Generation.Parts
{
    public static class VariableGenerator
    {
        public static async Task<TSyntaxNode> GenerateRepositoryVariable<TSyntaxNode>(this GenerationService service, 
            TSyntaxNode entity, 
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            var result = await service.Save(entity, new VariableDeclarationSyntaxBuilder()
                .WithName("_repository")
                .WithModifier(SyntaxKind.PrivateKeyword)
                .ForType(service.GenerationContext.EntityName.ToRepositoryInterface())
                .WithIsReadonly(true), fullFilename);

            return result;
        }

        public static async Task<TSyntaxNode> GenerateProjectionServiceVariable<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            var result = await service.Save(entity, new VariableDeclarationSyntaxBuilder()
                .WithName("_projectionService")
                .WithModifier(SyntaxKind.PrivateKeyword)
                .ForType("IProjectionService")
                .WithIsReadonly(true), fullFilename);

            return result;
        }
       
        public static async Task<TSyntaxNode> GenerateMapperVariable<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            var result = await service.Save(entity, new VariableDeclarationSyntaxBuilder()
                .WithName("_mapper")
                .WithModifier(SyntaxKind.PrivateKeyword)
                .ForType("IMapper")
                .WithIsReadonly(true), fullFilename);

            return result;
        }


        public static async Task<TSyntaxNode> GenerateLoggerVariable<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string loggerGenericType,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            var result = await service.Save(entity, new VariableDeclarationSyntaxBuilder()
                .WithName("_logger")
                .WithModifier(SyntaxKind.PrivateKeyword)
                .ForType($"ILogger<{loggerGenericType}>")
                .WithIsReadonly(true), fullFilename);

            return result;
        }
        public static async Task<TSyntaxNode> GenerateCqrsServiceVariable<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            var result = await service.Save(entity, new VariableDeclarationSyntaxBuilder()
                .WithName("_cqrsService")
                .WithModifier(SyntaxKind.PrivateKeyword)
                .ForType(service.GenerationContext.CqrsServiceContractName.ToInterface())
                .WithIsReadonly(true), fullFilename);

            return result;
        }
        //private readonly IAssetsCqrsService _assetsCqrsService;

        //private readonly ILogger<AssetsController> _logger;

    }

    //public static class MethodGenerator
    //{
    //    public static async Task<TSyntaxNode> GenerateUnitTestEstablishContext<TSyntaxNode>(this GenerationService service,
    //        TSyntaxNode entity,
    //        string fullFilename)

    //        where TSyntaxNode : TypeDeclarationSyntax
    //    {
    //        return await service.Save(entity, new MethodDeclarationSyntaxBuilder()
    //            .WithName("Establish_context")
    //            .IsOverride(true)
    //            .WithModifier(SyntaxKind.ProtectedKeyword)
    //            .AddStatement(new StatementSyntaxBuilder().AddStatement("base.Establish_context();")));
    //    }
    //}
}
