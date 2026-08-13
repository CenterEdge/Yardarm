using Microsoft.OpenApi;
using Microsoft.CodeAnalysis;
using Yardarm.Spec;

namespace Yardarm.Enrichment
{
    public interface IOpenApiSyntaxNodeEnricher<TSyntaxNode, TElement> : IEnricher<TSyntaxNode, OpenApiEnrichmentContext<TElement>>, IOpenApiSyntaxNodeEnricher
        where TSyntaxNode : SyntaxNode
        where TElement : IOpenApiElement
    {
    }
}
