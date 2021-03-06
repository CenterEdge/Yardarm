using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yardarm.Enrichment.Internal;

namespace Yardarm.Enrichment
{
    public static class EnrichmentExtensions
    {
        public static IEnumerable<T> Sort<T>(this IEnumerable<T> enrichers)
            where T : IEnricher =>
            EnricherSorter.Default.Sort(enrichers);

        public static TTarget Enrich<TTarget>(this TTarget target, IEnumerable<IEnricher<TTarget>> enrichers) =>
            enrichers
                .Sort()
                .Aggregate(target, (p, enricher) => enricher.Enrich(p));

        public static TTarget Enrich<TTarget, TContext>(this TTarget target, IEnumerable<IEnricher<TTarget, TContext>> enrichers, TContext context) =>
            enrichers
                .Sort()
                .Aggregate(target, (p, enricher) => enricher.Enrich(p, context));

        public static ValueTask<TTarget> EnrichAsync<TTarget>(this TTarget target,
            IEnumerable<IAsyncEnricher<TTarget>> enrichers,
            CancellationToken cancellationToken = default) =>
            enrichers
                .Sort()
                .ToAsyncEnumerable()
                .AggregateAwaitWithCancellationAsync(target, (p, enricher, ct) => enricher.EnrichAsync(p, ct),
                    cancellationToken);

        public static ValueTask<TTarget> EnrichAsync<TTarget, TContext>(this TTarget target,
            IEnumerable<IAsyncEnricher<TTarget, TContext>> enrichers,
            TContext context, CancellationToken cancellationToken = default) =>
            enrichers
                .Sort()
                .ToAsyncEnumerable()
                .AggregateAwaitWithCancellationAsync(target, (p, enricher, ct) => enricher.EnrichAsync(p, context, ct),
                    cancellationToken);
    }
}
