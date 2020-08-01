namespace Yardarm.Enrichment
{
    public interface IEnricher<TTarget>
    {
        TTarget Enrich(TTarget target);

        int Priority { get; }
    }
}
