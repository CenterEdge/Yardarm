using Yardarm.Enrichment.Requests;
using Yardarm.Enrichment.Responses;
using Yardarm.Enrichment.Schema;

namespace Yardarm.Enrichment
{
    public interface IEnrichers
    {
        IRequestEnrichers Requests { get; }
        IResponseEnrichers Responses { get; }
    }
}
