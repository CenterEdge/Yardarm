using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Enrichment.Compilation
{
    /// <summary>
    /// Enricher for compilation units which were parsed from assembly resource files. The context includes
    /// the name of the resource.
    /// </summary>
    public interface IResourceFileEnricher : IEnricher<CompilationUnitSyntax, ResourceFileEnrichmentContext>
    {
        /// <summary>
        /// Returns true if the <see cref="Enrich"/> method should be called for a given resource name.
        /// </summary>
        /// <param name="resourceName">The name of the resource which may be enriched.</param>
        /// <returns>True if <see cref="Enrich"/> should be called.</returns>
        public bool ShouldEnrich(string resourceName);
    }
}
