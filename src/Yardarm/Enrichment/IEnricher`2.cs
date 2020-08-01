namespace Yardarm.Enrichment
{
    public interface IEnricher<TTarget, in TContext>
    {
        TTarget Enrich(TTarget target, TContext context);

        int Priority { get; }
    }
}
