using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Yardarm.Enrichment.Compilation
{
    public class ResourceFileEnrichmentContext
    {
        public CSharpCompilation Compilation { get; }

        public SyntaxTree SyntaxTree { get; }

        public string ResourceName { get; }

        public ResourceFileEnrichmentContext(CSharpCompilation compilation, SyntaxTree syntaxTree,
            string resourceName)
        {
            ArgumentNullException.ThrowIfNull(compilation);
            ArgumentNullException.ThrowIfNull(syntaxTree);
            ArgumentNullException.ThrowIfNull(resourceName);

            Compilation = compilation;
            SyntaxTree = syntaxTree;
            ResourceName = resourceName;
        }
    }
}
