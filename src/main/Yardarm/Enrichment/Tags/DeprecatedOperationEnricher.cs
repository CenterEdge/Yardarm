using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Tags
{
    /// <summary>
    /// Marks deprecated operations with <see cref="System.ObsoleteAttribute"/>.
    /// </summary>
    public class DeprecatedOperationEnricher(
        IOperationNameProvider operationNameProvider)
        : IOpenApiSyntaxNodeEnricher<MethodDeclarationSyntax, OpenApiOperation>
    {
        public MethodDeclarationSyntax Enrich(MethodDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiOperation> context) =>
            context.Element.Deprecated
                ? MarkObsolete(target, operationNameProvider.GetOperationName(context.LocatedElement))
                : target;

        private static MethodDeclarationSyntax MarkObsolete(MethodDeclarationSyntax target, string? operationName) =>
            target.AddAttributeLists(AttributeList(SingletonSeparatedList(
                Attribute(WellKnownTypes.System.ObsoleteAttribute.Name,
                    AttributeArgumentList(SingletonSeparatedList(AttributeArgument(
                        SyntaxHelpers.StringLiteral($"Operation {operationName} has been marked deprecated.")))))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed));

    }
}
