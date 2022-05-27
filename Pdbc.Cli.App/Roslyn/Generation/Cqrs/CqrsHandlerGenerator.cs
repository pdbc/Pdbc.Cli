using System.Threading.Tasks;
using Pdbc.Cli.App.Context;
using Pdbc.Cli.App.Extensions;
using Pdbc.Cli.App.Roslyn.Builders;
using Pdbc.Cli.App.Roslyn.Builders.ExpressionBuilders;
using Pdbc.Cli.App.Roslyn.Builders.SyntaxBuilders;
using Pdbc.Cli.App.Roslyn.Extensions;
using Pdbc.Cli.App.Roslyn.Generation.Parts;

namespace Pdbc.Cli.App.Roslyn.Generation.Cqrs
{
    public static class CqrsHandlerGenerator
    {

        public static async Task GenerateCqrsHandlerClass(this GenerationService service)
        {
            var className = service.GenerationContext.ActionInfo.CqrsInputClassName.ToHandler();
            var subfolders = new[]
            {
                "CQRS",
                service.GenerationContext.PluralEntityName,
                service.GenerationContext.ActionName
            };

            var roslynProjectContext = service.RoslynSolutionContext.GetRoslynProjectContextFor("Core");
            var fullFilename = roslynProjectContext.GetFullFilenameFor(className, subfolders);

            var entity = await roslynProjectContext.GetClassByName(className);

            var outputCqrsClassname = service.GenerationContext.ActionInfo.CqrsOutputClassNameOverride;
            var inputCqrsClassName = service.GenerationContext.ActionInfo.CqrsInputClassName;

            if (entity == null)
            {
                var entityNamespace = roslynProjectContext.GetNamespace(subfolders);

                entity = new ClassDeclarationSyntaxBuilder()
                    .WithName(className)
                    .ForNamespace(entityNamespace)
                    .AddUsingStatement("System.Threading")
                    .AddUsingStatement("System.Threading.Tasks")
                    .AddUsingStatement("AutoMapper")
                    .AddUsingStatement("MediatR")
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDomainModel())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForData())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDataRepositories())
                    .AddUsingStatement(service.GenerationContext.GetNamespaceForDto())
                    .AddUsingAertssenFrameworkCqrsInfra()
                    .AddUsingAertssenFrameworkServices()
                    .AddBaseClass(
                        $"IRequestHandler<{inputCqrsClassName}, {outputCqrsClassname}>")
                    .Build();

                await service.FileHelperService.WriteFile(fullFilename, entity);
            }


            // List Query Handler
            if (service.GenerationContext.ActionInfo.IsListAction)
            {
                entity = await service.GenerateRepositoryVariable(entity, fullFilename);
                entity = await service.GenerateProjectionServiceVariable(entity, fullFilename);

                entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                        .AddParameter(service.GenerationContext.EntityName.ToRepositoryInterface(), "repository")
                        .AddParameter("IProjectionService", "projectionService")
                        .AddStatement(new AssignmentSyntaxBuilder("_repository", "repository"))
                        .AddStatement(new AssignmentSyntaxBuilder("_projectionService", "projectionService")),
                    fullFilename);


                entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.CqrsHandleMethod(inputCqrsClassName, outputCqrsClassname)
                        .AddStatement(new AssignmentSyntaxBuilder("var items", $"_repository.GetAll()"))
                        .AddStatement(new AssignmentSyntaxBuilder("var mappedItems",
                            $"_projectionService.Project<{service.GenerationContext.EntityName},{service.GenerationContext.EntityName.ToDataDto()}>(items)"))
                        .AddStatement(new ReturnStatementSyntaxBuilder().WithName("mappedItems")),
                    fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsGetAction)
            {
                entity = await service.GenerateRepositoryVariable(entity, fullFilename);
                entity = await service.GenerateMapperVariable(entity, fullFilename);

                entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                        .AddParameter(service.GenerationContext.EntityName.ToRepositoryInterface(), "repository")
                        .AddParameter("IMapper", "mapper")
                        .AddStatement(new AssignmentSyntaxBuilder("_repository", "repository"))
                        .AddStatement(new AssignmentSyntaxBuilder("_mapper", "mapper")),
                    fullFilename);

                entity = await service.Save(entity, MethodDeclarationSyntaxBuilder.CqrsHandleMethod(inputCqrsClassName, outputCqrsClassname)
                        .AddStatement(new StatementSyntaxBuilder("var item = await _repository.GetByIdAsync(request.Id);"))
                        .AddStatement(new StatementSyntaxBuilder($"var mappedItem =_mapper.Map<{service.GenerationContext.EntityName.ToDataDto()}>(item);"))
                        .AddStatement(new ReturnStatementSyntaxBuilder().WithObjectCreationExpression(
                            new ObjectCreationExpressionSyntaxBuilder(
                                    $"{service.GenerationContext.ActionInfo.CqrsOutputClassName}")
                                .AddAssignementStatement($"{service.GenerationContext.EntityName}", "mappedItem"))),

                    fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsDeleteAction)
            {
                entity = await service.GenerateRepositoryVariable(entity, fullFilename);

                entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                        .AddParameter(service.GenerationContext.EntityName.ToRepositoryInterface(), "repository")
                        .AddStatement(new AssignmentSyntaxBuilder("_repository", "repository")),
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Handle")
                        .Async()
                        .WithReturnType($"Task<{outputCqrsClassname}>")
                        .AddParameter(inputCqrsClassName, "request")
                        .AddParameter("CancellationToken", "cancellationToken")
                        .AddStatement(new AssignmentSyntaxBuilder("var item",
                            $"await _repository.GetByIdAsync(request.Id)"))
                        .AddStatement(new StatementSyntaxBuilder("_repository.Delete(item);"))
                        //.AddStatement(new ReturnStatementSyntaxBuilder().WithName($"new {_generationContext.CqrsOutputClassName}()"+"{"+$"{_generationContext.EntityName} = mappedItem"+"}")),
                        .AddStatement(new ReturnStatementSyntaxBuilder().WithName("await Nothing.AtAllAsync()")),

                    fullFilename);
            }
            else if (service.GenerationContext.ActionInfo.IsStoreAction)
            {
                entity = await service.GenerateRepositoryVariable(entity, fullFilename);
                entity = await service.GenerateFactoryVariable(entity, fullFilename);
                entity = await service.GenerateChangesHandlerVariable(entity, fullFilename);
                entity = await service.GenerateMapperVariable(entity, fullFilename);
                entity = await service.GenerateDbContextServiceVariable(entity, fullFilename);
                //entity = await service.Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_changesHandler")
                //    .ForType($"IChangesHandler<{service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToInterface()}, {service.GenerationContext.EntityName}>")
                //    .WithIsReadonly(true), fullFilename);

                //entity = await service.Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_mapper")
                //    .ForType("IMapper")
                //    .WithIsReadonly(true), fullFilename);
                //entity = await service.Save(entity, new VariableDeclarationSyntaxBuilder().WithName("_dbContextService")
                //    .ForType($"I{service.GenerationContext.ApplicationName}DbContextService")
                //    .WithIsReadonly(true), fullFilename);

                entity = await service.Save(entity, new ConstructorDeclarationSyntaxBuilder().WithName(className)
                        .AddParameter(
                            $"IFactory<{service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToInterface()}, {service.GenerationContext.EntityName}>",
                            "factory")
                        .AddParameter(
                            $"IChangesHandler<{service.GenerationContext.ActionInfo.EntityActionName.ToDto().ToInterface()}, {service.GenerationContext.EntityName}>",
                            "changesHandler")
                        .AddParameter(service.GenerationContext.EntityName.ToRepositoryInterface(), "repository")
                        .AddParameter($"IMapper", "mapper")
                        .AddParameter($"I{service.GenerationContext.ApplicationName}DbContextService", "dbContextService")
                        .AddStatement(new AssignmentSyntaxBuilder("_factory", "factory"))
                        .AddStatement(new AssignmentSyntaxBuilder("_changesHandler", "changesHandler"))
                        .AddStatement(new AssignmentSyntaxBuilder("_repository", "repository"))
                        .AddStatement(new AssignmentSyntaxBuilder("_mapper", "mapper"))
                        .AddStatement(new AssignmentSyntaxBuilder("_dbContextService", "dbContextService"))
                    ,
                    fullFilename);

                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Handle")
                        .Async()
                        .WithReturnType($"Task<{outputCqrsClassname}>")
                        .AddParameter(inputCqrsClassName, "request")
                        .AddParameter("CancellationToken", "cancellationToken")
                        .AddStatement(new StatementSyntaxBuilder($"{service.GenerationContext.EntityName} entity;"))
                        .AddStatement(
                            new StatementSyntaxBuilder($"if (request.{service.GenerationContext.EntityName}.Id.HasValue) "))
                        .AddStatement(new BlockStatementSyntaxBuilder()
                            .AddStatement(new StatementSyntaxBuilder(
                                $"   entity = _repository.GetById(request.{service.GenerationContext.EntityName}.Id.Value);"))
                            .AddStatement(new StatementSyntaxBuilder(
                                $"   _changesHandler.ApplyChanges(entity, request.{service.GenerationContext.EntityName});"))
                            .AddStatement(new StatementSyntaxBuilder($"   _repository.Update(entity);"))
                        )
                        .AddStatement(new StatementSyntaxBuilder($" else "))
                        .AddStatement(new BlockStatementSyntaxBuilder()
                            .AddStatement(new StatementSyntaxBuilder(
                                $"   entity = _factory.Create(request.{service.GenerationContext.EntityName});"))
                            .AddStatement(new StatementSyntaxBuilder($"   _repository.Insert(entity);"))
                        )
                        .AddStatement(new StatementSyntaxBuilder($""))
                        .AddStatement(
                            new StatementSyntaxBuilder($"await _dbContextService.SaveChangesAsync(cancellationToken);"))
                        .AddStatement(new StatementSyntaxBuilder(
                            $" return new {service.GenerationContext.ActionInfo.CqrsOutputClassName} {{ {service.GenerationContext.EntityName} = _mapper.Map<{service.GenerationContext.EntityName.ToDataDto()}>(entity) }};"))
                    ,

                    fullFilename);
            }
            else
            {
                entity = await service.Save(entity, new MethodDeclarationSyntaxBuilder()
                        .WithName("Handle")
                        .WithReturnType($"Task<{outputCqrsClassname}>")
                        .AddParameter(inputCqrsClassName, "request")
                        .AddParameter("CancellationToken", "cancellationToken")
                        .ThrowsNewNotImplementedException(),
                    fullFilename);
            }



        }
    }
}