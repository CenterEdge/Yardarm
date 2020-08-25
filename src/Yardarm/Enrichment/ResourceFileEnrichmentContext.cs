using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Yardarm.Enrichment
{
    public class ResourceFileEnrichmentContext
    {
        public CSharpCompilation Compilation { get; }

        public SyntaxTree SyntaxTree { get; }

        public string ResourceName { get; }

        public ResourceFileEnrichmentContext(CSharpCompilation compilation, SyntaxTree syntaxTree,
            string resourceName)
        {
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            SyntaxTree = syntaxTree ?? throw new ArgumentNullException(nameof(syntaxTree));
            ResourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
        }
    }
}
