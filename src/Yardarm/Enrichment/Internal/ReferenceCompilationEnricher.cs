using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Yardarm.Generation;

namespace Yardarm.Enrichment.Internal
{
    internal class ReferenceCompilationEnricher : ICompilationEnricher
    {
        private readonly IList<IReferenceGenerator> _referenceGenerators;

        public int Priority => CompilationEnrichmentPriority.References;

        public ReferenceCompilationEnricher(IEnumerable<IReferenceGenerator> referenceGenerators)
        {
            _referenceGenerators = referenceGenerators.ToArray();
        }

        public async ValueTask<CSharpCompilation> EnrichAsync(CSharpCompilation target, CancellationToken cancellationToken = default)
        {
            List<MetadataReference> references = await _referenceGenerators
                .ToAsyncEnumerable()
                .SelectMany(p => p.Generate(cancellationToken))
                .ToListAsync(cancellationToken);

            return target.AddReferences(references);
        }
    }
}
