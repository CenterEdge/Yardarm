using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment.Compilation;
using Yardarm.Spec;
using Yardarm.Generation;
using Yardarm.Generation.Tag;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.MicrosoftExtensionsHttp.Internal
{
    /// <summary>
    /// Adds statements to register all APIs to the "AddAllApisInternal" method.
    /// </summary>
    internal class ApiBuilderExtensionsEnricher : IResourceFileEnricher
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiTag> _tagGeneratorRegistry;
        private readonly ITypeGeneratorRegistry<OpenApiTag, TagImplementationCategory> _tagImplementationGeneratorRegistry;

        public ApiBuilderExtensionsEnricher(OpenApiDocument document,
            ITypeGeneratorRegistry<OpenApiTag> tagGeneratorRegistry,
            ITypeGeneratorRegistry<OpenApiTag, TagImplementationCategory> tagImplementationGeneratorRegistry)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(tagGeneratorRegistry);
            ArgumentNullException.ThrowIfNull(tagImplementationGeneratorRegistry);

            _document = document;
            _tagGeneratorRegistry = tagGeneratorRegistry;
            _tagImplementationGeneratorRegistry = tagImplementationGeneratorRegistry;
        }

        public bool ShouldEnrich(string resourceName) =>
            resourceName == "Yardarm.MicrosoftExtensionsHttp.Client.ApiBuilderExtensions.cs";

        public CompilationUnitSyntax Enrich(CompilationUnitSyntax target, ResourceFileEnrichmentContext context)
        {
            var method = target
                .DescendantNodes(node => node is MemberDeclarationSyntax or CompilationUnitSyntax)
                .OfType<MethodDeclarationSyntax>()
                .Single(p => p.Identifier.ValueText == "AddAllApisInternal");

            var newMethod = method.WithBody(Block(GenerateApiStatements().ToArray()));

            return target.ReplaceNode(method, newMethod);
        }

        private IEnumerable<StatementSyntax> GenerateApiStatements()
        {
            var tags = _document.Paths.ToLocatedElements()
                .GetOperations()
                .GetTags()
                .Distinct(TagComparer.Instance);

            foreach (var tag in tags)
            {
                TypeSyntax interfaceName = _tagGeneratorRegistry.Get(tag).TypeInfo.Name;
                TypeSyntax implementationName = _tagImplementationGeneratorRegistry.Get(tag).TypeInfo.Name;

                yield return ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("builder"),
                            GenericName(
                                Identifier("AddApi"),
                                TypeArgumentList(SeparatedList(new[]
                                {
                                    interfaceName,
                                    implementationName
                                })))),
                        ArgumentList(SeparatedList(new[]
                        {
                            Argument(IdentifierName("configureClient")),
                            Argument(IdentifierName("skipIfAlreadyRegistered"))
                        }))),
                    Token(SyntaxKind.SemicolonToken))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed);
            }
        }
    }
}
