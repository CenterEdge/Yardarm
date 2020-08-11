using System.Collections.Generic;
using System.Linq;

namespace Yardarm.Enrichment.Responses.Internal
{
    internal class ResponseEnrichers : IResponseEnrichers
    {
        public IList<IResponseClassEnricher> Class { get; }
        public IList<IResponseHeaderPropertyEnricher> HeaderProperty { get; }

        public ResponseEnrichers(
            IEnumerable<IResponseClassEnricher> responseClassEnrichers,
            IEnumerable<IResponseHeaderPropertyEnricher> headerPropertyEnrichers)
        {
            Class = responseClassEnrichers.ToArray();
            HeaderProperty = headerPropertyEnrichers.ToArray();
        }
    }
}
