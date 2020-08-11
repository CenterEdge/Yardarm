using System.Collections.Generic;

namespace Yardarm.Enrichment.Responses
{
    public interface IResponseEnrichers
    {
        IList<IResponseHeaderPropertyEnricher> HeaderProperty { get; }
    }
}
