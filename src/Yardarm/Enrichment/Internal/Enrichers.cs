using System;
using Yardarm.Enrichment.Requests;
using Yardarm.Enrichment.Responses;
using Yardarm.Enrichment.Schema;

namespace Yardarm.Enrichment.Internal
{
    internal class Enrichers : IEnrichers
    {
        public IRequestEnrichers Requests { get; }
        public IResponseEnrichers Responses { get; }
        public ISchemaEnrichers Schema { get; }

        public Enrichers(
            IRequestEnrichers requestEnrichers,
            IResponseEnrichers responseEnrichers,
            ISchemaEnrichers schemaEnrichers)
        {
            Requests = requestEnrichers ?? throw new ArgumentNullException(nameof(requestEnrichers));
            Responses = responseEnrichers ?? throw new ArgumentNullException(nameof(responseEnrichers));
            Schema = schemaEnrichers ?? throw new ArgumentNullException(nameof(schemaEnrichers));
        }
    }
}
