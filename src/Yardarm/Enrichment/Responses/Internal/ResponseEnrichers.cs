using System.Collections.Generic;
using System.Linq;

namespace Yardarm.Enrichment.Responses.Internal
{
    internal class ResponseEnrichers : IResponseEnrichers
    {
        public IList<IResponseHeaderPropertyEnricher> HeaderProperty { get; }

        public ResponseEnrichers(
            IEnumerable<IResponseHeaderPropertyEnricher> headerPropertyEnrichers)
        {
            HeaderProperty = headerPropertyEnrichers.ToArray();
        }
    }
}
