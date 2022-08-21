namespace Yardarm.Enrichment
{
    public interface IEnricher<TTarget> : IEnricher
    {
        TTarget Enrich(TTarget target);
    }
}
