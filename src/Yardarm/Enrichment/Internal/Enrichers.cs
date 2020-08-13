using System;
using Yardarm.Enrichment.Responses;

namespace Yardarm.Enrichment.Internal
{
    internal class Enrichers : IEnrichers
    {
        public IResponseEnrichers Responses { get; }

        public Enrichers(
            IResponseEnrichers responseEnrichers)
        {
            Responses = responseEnrichers ?? throw new ArgumentNullException(nameof(responseEnrichers));
        }
    }
}
