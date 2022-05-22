using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;

namespace Pdbc.Cli.App.Roslyn.Generation.Parts
{
    public static class MethodGenerator
    {
        public static async Task<TSyntaxNode> GenerateCqrsStandardCommandMethod<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            return await service.Save(entity, MethodDeclarationSyntaxBuilder.OperationNameMethodAsync(service.GenerationContext.ActionInfo.ActionOperationName,
                        service.GenerationContext.ActionInfo.ApiRequestClassName,
                        service.GenerationContext.ActionInfo.ApiResponseClassNameOverride)
                    .AddStatement(new StatementSyntaxBuilder(
                        $"return await Command<{service.GenerationContext.ActionInfo.ApiRequestClassName}, {service.GenerationContext.ActionInfo.CqrsInputClassName}, {service.GenerationContext.ActionInfo.CqrsOutputClassNameOverride}, {service.GenerationContext.ActionInfo.ApiResponseClassNameOverride}>(request); ")),
                fullFilename);
        }

        public static async Task<TSyntaxNode> GenerateCqrsStandardQueryMethod<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            return await service.Save(entity, MethodDeclarationSyntaxBuilder.OperationNameMethodAsync(service.GenerationContext.ActionInfo.ActionOperationName,
                        service.GenerationContext.ActionInfo.ApiRequestClassName,
                        service.GenerationContext.ActionInfo.ApiResponseClassNameOverride)
                    .AddStatement(new StatementSyntaxBuilder(
                        $"return await Query<{service.GenerationContext.ActionInfo.ApiRequestClassName}, {service.GenerationContext.ActionInfo.CqrsInputClassName}, {service.GenerationContext.ActionInfo.CqrsOutputClassNameOverride}, {service.GenerationContext.ActionInfo.ApiResponseClassNameOverride}>(request); ")),
                fullFilename);
        }

        public static async Task<TSyntaxNode> GenerateVerifyResponseNotImplementedExceptionMethod<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            return await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                .WithName("VerifyResponse")
                .AddParameter(service.GenerationContext.ActionInfo.ApiResponseClassNameOverride, "response")
                .IsOverride(true)
                .AddStatement(
            new StatementSyntaxBuilder($"response.Notifications?.HasErrors().ShouldBeFalse();"))
                .AddStatement(
            new StatementSyntaxBuilder($"throw new NotImplementedException();"))
                //
                ,
            fullFilename);
        }

    }
}