using System;
using System.Collections.Generic;
using System.Linq;
using Yardarm.Enrichment.Schema;

namespace Yardarm.Enrichment.Internal
{
    internal class Enrichers : IEnrichers
    {
        public IList<IOperationClassMethodEnricher> OperationClassMethod { get; }
        public IList<IOperationInterfaceMethodEnricher> OperationInterfaceMethod { get; }
        public ISchemaEnrichers Schema { get; }

        public Enrichers(
            IEnumerable<IOperationClassMethodEnricher> operationClassMethodEnrichers,
            IEnumerable<IOperationInterfaceMethodEnricher> operationInterfaceMethodEnrichers,
            ISchemaEnrichers schemaEnrichers)
        {
            OperationClassMethod = operationClassMethodEnrichers.ToArray();
            OperationInterfaceMethod = operationInterfaceMethodEnrichers.ToArray();
            Schema = schemaEnrichers ?? throw new ArgumentNullException(nameof(schemaEnrichers));
        }
    }
}
