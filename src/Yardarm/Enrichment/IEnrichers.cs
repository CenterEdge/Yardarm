using System.Collections.Generic;
using Yardarm.Enrichment.Schema;

namespace Yardarm.Enrichment
{
    public interface IEnrichers
    {
        ISchemaEnrichers Schema { get; }
        IList<IOperationMethodEnricher> OperationMethod { get; }
    }
}
