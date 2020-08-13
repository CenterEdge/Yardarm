using Yardarm.Enrichment.Responses;

namespace Yardarm.Enrichment
{
    public interface IEnrichers
    {
        IResponseEnrichers Responses { get; }
    }
}
