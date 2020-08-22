using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Tags.Internal
{
    /// <summary>
    /// Marks deprecated operations with <see cref="System.ObsoleteAttribute"/>.
    /// </summary>
    internal class DeprecatedOperationEnricher : IOpenApiSyntaxNodeEnricher<MethodDeclarationSyntax, OpenApiOperation>
    {
        public int Priority => 0;

        public MethodDeclarationSyntax Enrich(MethodDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiOperation> context) =>
            context.Element.Deprecated
                ? MarkObsolete(target, context.Element.OperationId)
                : target;

        private static MethodDeclarationSyntax MarkObsolete(MethodDeclarationSyntax target, string operationId) =>
            target.AddAttributeLists(AttributeList(SingletonSeparatedList(
                Attribute(WellKnownTypes.System.ObsoleteAttribute.Name,
                    AttributeArgumentList(SingletonSeparatedList(AttributeArgument(
                        SyntaxHelpers.StringLiteral($"Operation {operationId} has been marked deprecated."))))))));

    }
}
