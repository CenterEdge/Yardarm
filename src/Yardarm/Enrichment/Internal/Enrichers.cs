using System;
using System.Collections.Generic;
using System.Linq;
using Yardarm.Enrichment.Schema;

namespace Yardarm.Enrichment.Internal
{
    internal class Enrichers : IEnrichers
    {
        public IList<IOperationMethodEnricher> OperationMethod { get; }
        public ISchemaEnrichers Schema { get; }

        public Enrichers(
            IEnumerable<IOperationMethodEnricher> operationMethodEnrichers,
            ISchemaEnrichers schemaEnrichers)
        {
            OperationMethod = operationMethodEnrichers.ToArray();
            Schema = schemaEnrichers ?? throw new ArgumentNullException(nameof(schemaEnrichers));
        }
    }
}
