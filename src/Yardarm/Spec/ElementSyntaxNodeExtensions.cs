using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Spec
{
    public static class ElementSyntaxNodeExtensions
    {
        public static TSyntaxNode AddElementAnnotation<TSyntaxNode, TElement>(this TSyntaxNode node,
            LocatedOpenApiElement<TElement> element, IOpenApiElementRegistry elementRegistry)
            where TSyntaxNode : SyntaxNode
            where TElement : IOpenApiSerializable =>
            node.WithAdditionalAnnotations(
                new SyntaxAnnotation(typeof(TElement).Name, elementRegistry.Add(element)));

        public static LocatedOpenApiElement<TElement>? GetElementAnnotation<TElement>(this SyntaxNode node,
            IOpenApiElementRegistry elementRegistry)
            where TElement : IOpenApiSerializable
        {
            string? key = node.GetAnnotations(typeof(TElement).Name).FirstOrDefault()?.Data;
            if (key == null)
            {
                return null;
            }

            return elementRegistry.TryGet<TElement>(key, out var element)
                ? element
                : null;
        }
    }
}
