using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yardarm.Enrichment.Internal;

namespace Yardarm.Enrichment;

public static class EnrichmentExtensions
{
    extension<T>(IEnumerable<T> enrichers)
        where T : IEnricher
    {
        public IEnumerable<T> Sort() =>
            EnricherSorter.Default.Sort(enrichers);
    }

    extension<TTarget>(TTarget target)
    {
        public TTarget Enrich(IEnumerable<IEnricher<TTarget>> enrichers) =>
            enrichers
                .Sort()
                .Aggregate(target, (p, enricher) => enricher.Enrich(p));

        public TTarget Enrich<TContext>(IEnumerable<IEnricher<TTarget, TContext>> enrichers, TContext context) =>
            enrichers
                .Sort()
                .Aggregate(target, (p, enricher) => enricher.Enrich(p, context));

        public ValueTask<TTarget> EnrichAsync(IEnumerable<IAsyncEnricher<TTarget>> enrichers,
            CancellationToken cancellationToken = default) =>
            enrichers
                .Sort()
                .ToAsyncEnumerable()
                .AggregateAsync(target, (p, enricher, ct) => enricher.EnrichAsync(p, ct),
                    cancellationToken);

        public ValueTask<TTarget> EnrichAsync<TContext>(IEnumerable<IAsyncEnricher<TTarget, TContext>> enrichers,
            TContext context, CancellationToken cancellationToken = default) =>
            enrichers
                .Sort()
                .ToAsyncEnumerable()
                .AggregateAsync(target, (p, enricher, ct) => enricher.EnrichAsync(p, context, ct),
                    cancellationToken);
    }
}
