using System;
using System.Collections.Generic;
using System.Linq;
using Yardarm.Enrichment.Responses;
using Yardarm.Enrichment.Schema;

namespace Yardarm.Enrichment.Internal
{
    internal class Enrichers : IEnrichers
    {
        public IList<IOperationClassMethodEnricher> OperationClassMethod { get; }
        public IList<IOperationInterfaceMethodEnricher> OperationInterfaceMethod { get; }

        public IResponseEnrichers Responses { get; }
        public ISchemaEnrichers Schema { get; }

        public Enrichers(
            IEnumerable<IOperationClassMethodEnricher> operationClassMethodEnrichers,
            IEnumerable<IOperationInterfaceMethodEnricher> operationInterfaceMethodEnrichers,
            IResponseEnrichers responseEnrichers,
            ISchemaEnrichers schemaEnrichers)
        {
            OperationClassMethod = operationClassMethodEnrichers.ToArray();
            OperationInterfaceMethod = operationInterfaceMethodEnrichers.ToArray();
            Responses = responseEnrichers ?? throw new ArgumentNullException(nameof(responseEnrichers));
            Schema = schemaEnrichers ?? throw new ArgumentNullException(nameof(schemaEnrichers));
        }
    }
}
