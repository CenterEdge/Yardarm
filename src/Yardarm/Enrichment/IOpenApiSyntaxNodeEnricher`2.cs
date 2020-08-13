using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Enrichment
{
    public interface IOpenApiSyntaxNodeEnricher<TSyntaxNode, TElement> : IEnricher<TSyntaxNode, LocatedOpenApiElement<TElement>>, IOpenApiSyntaxNodeEnricher
        where TSyntaxNode : SyntaxNode
        where TElement : IOpenApiSerializable
    {
    }
}
