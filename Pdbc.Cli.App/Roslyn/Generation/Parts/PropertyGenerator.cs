using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;

namespace Pdbc.Cli.App.Roslyn.Generation.Parts
{
    public static class PropertyGenerator
    {
        public static async Task<TSyntaxNode> GenerateDataDtoInterfaceProperty<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            //public IAddressDataDto Address { get; set; }
            return await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                .WithName(service.GenerationContext.EntityName)
                .ForType(service.GenerationContext.EntityName.ToDataDto().ToInterface()), fullFilename);

            //return await service.Save(entity, new PropertyDeclarationSyntaxBuilder("String", "ExternalSystem"), fullFilename);
        }
        public static async Task<TSyntaxNode> GenerateIdentifierOptionalProperty<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            return await service.Save(entity, new PropertyDeclarationSyntaxBuilder().WithName("Id").ForType("long?"), fullFilename);

            //return await service.Save(entity, new PropertyDeclarationSyntaxBuilder("String", "ExternalSystem"), fullFilename);
        }
        public static async Task<TSyntaxNode> GenerateIdentifierMandatoryProperty<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            return await service.Save(entity, new PropertyDeclarationSyntaxBuilder().WithName("Id").ForType("long"), fullFilename);

            //return await service.Save(entity, new PropertyDeclarationSyntaxBuilder("String", "ExternalSystem"), fullFilename);
        }

        public static async Task<TSyntaxNode> GenerateActionDtoInterfaceProperty<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            return await service.Save(entity,
                new PropertyDeclarationSyntaxBuilder().WithName(service.GenerationContext.EntityName)
                    .ForType(service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToInterface()), fullFilename);
        }
        public static async Task<TSyntaxNode> GenerateExternalSystemProperty<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            return await service.Save(entity, new PropertyDeclarationSyntaxBuilder("String", "ExternalSystem"), fullFilename);
        }
        public static async Task<TSyntaxNode> GenerateExternalIdentificationProperty<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            return await service.Save(entity, new PropertyDeclarationSyntaxBuilder("String", "ExternalIdentification"), fullFilename);
        }
        public static async Task<TSyntaxNode> GenerateDateModifiedProperty<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            return await service.Save(entity, new PropertyDeclarationSyntaxBuilder("DateTimeOffset", "DateModified"), fullFilename);
        }
        public static async Task<TSyntaxNode> GenerateQueryProperty<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            var result = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                .WithModifier(SyntaxKind.ProtectedKeyword)
                .WithName("Query")
                .ForType(service.GenerationContext.ActionInfo.CqrsInputClassName), fullFilename);

            return result;
        }

        public static async Task<TSyntaxNode> GenerateCancellationTokenProperty<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            var result  = await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                .WithModifier(SyntaxKind.ProtectedKeyword)
                .WithName("CancellationToken")
                .ForType("CancellationToken"), fullFilename);


            return result;
        }

        public static async Task<TSyntaxNode> GenerateUnitTestRepositoryProperty<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            return await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                .WithModifier(SyntaxKind.ProtectedKeyword)
                .WithName("Repository")
                .ForType($"{service.GenerationContext.EntityName.ToRepositoryInterface()}")
                .WithDependencyType(service.GenerationContext.EntityName.ToRepositoryInterface()), fullFilename);
        }


        public static async Task<TSyntaxNode> GenerateUnitTestProjectionService<TSyntaxNode>(this GenerationService service,
            TSyntaxNode entity,
            string fullFilename)

            where TSyntaxNode : TypeDeclarationSyntax
        {
            return await service.Save(entity, new PropertyDeclarationSyntaxBuilder()
                .WithModifier(SyntaxKind.ProtectedKeyword)
                .WithName("ProjectionService")
                .ForType($"IProjectionService")
                .WithDependencyType("IProjectionService"), fullFilename);
        }
    }
}