using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Enrichment
{
    public class OpenApiEnrichmentContext<TElement>
        where TElement : IOpenApiSerializable
    {
        public CSharpCompilation Compilation { get; }

        public LocatedOpenApiElement<TElement> LocatedElement { get; }

        public TElement Element => LocatedElement.Element;

        public OpenApiEnrichmentContext(CSharpCompilation compilation, LocatedOpenApiElement<TElement> locatedElement)
        {
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            LocatedElement = locatedElement ?? throw new ArgumentNullException(nameof(locatedElement));
        }
    }
}
