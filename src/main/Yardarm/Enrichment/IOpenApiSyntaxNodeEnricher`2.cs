using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Enrichment
{
    public interface IOpenApiSyntaxNodeEnricher<TSyntaxNode, TElement> : IEnricher<TSyntaxNode, OpenApiEnrichmentContext<TElement>>, IOpenApiSyntaxNodeEnricher
        where TSyntaxNode : SyntaxNode
        where TElement : IOpenApiElement
    {
    }
}
