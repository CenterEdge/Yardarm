namespace Yardarm.Enrichment
{
    public interface IEnricher<TTarget, in TContext> : IEnricher
    {
        TTarget Enrich(TTarget target, TContext context);
    }
}
