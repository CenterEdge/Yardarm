using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Enrichment
{
    public class OpenApiEnrichmentContext<TElement>
        where TElement : IOpenApiElement
    {
        public CSharpCompilation Compilation { get; }

        public SyntaxTree SyntaxTree { get; }

        public LocatedOpenApiElement<TElement> LocatedElement { get; }

        public TElement Element => LocatedElement.Element;

        public OpenApiEnrichmentContext(CSharpCompilation compilation, SyntaxTree syntaxTree,
            LocatedOpenApiElement<TElement> locatedElement)
        {
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            SyntaxTree = syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree));
            LocatedElement = locatedElement ?? throw new ArgumentNullException(nameof(locatedElement));
        }
    }
}
