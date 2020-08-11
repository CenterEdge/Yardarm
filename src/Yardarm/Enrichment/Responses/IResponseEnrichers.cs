using System.Collections.Generic;

namespace Yardarm.Enrichment.Responses
{
    public interface IResponseEnrichers
    {
        IList<IResponseClassEnricher> Class { get; }
        IList<IResponseHeaderPropertyEnricher> HeaderProperty { get; }
    }
}
