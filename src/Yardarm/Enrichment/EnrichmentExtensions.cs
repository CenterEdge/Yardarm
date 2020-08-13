using System.Collections.Generic;
using System.Linq;

namespace Yardarm.Enrichment
{
    public static class EnrichmentExtensions
    {
        public static TTarget Enrich<TTarget>(this TTarget target, IEnumerable<IEnricher<TTarget>> enrichers) =>
            enrichers
                .OrderBy(p => p.Priority)
                .Aggregate(target, (p, enricher) => enricher.Enrich(p));

        public static TTarget Enrich<TTarget, TContext>(this TTarget target, IEnumerable<IEnricher<TTarget, TContext>> enrichers, TContext context) =>
            enrichers
                .OrderBy(p => p.Priority)
                .Aggregate(target, (p, enricher) => enricher.Enrich(p, context));
    }
}
