using System.Collections.Generic;
using Yardarm.Enrichment.Responses;
using Yardarm.Enrichment.Schema;

namespace Yardarm.Enrichment
{
    public interface IEnrichers
    {
        IResponseEnrichers Responses { get; }
        ISchemaEnrichers Schema { get; }
        IList<IOperationInterfaceMethodEnricher> OperationInterfaceMethod { get; }
        IList<IOperationClassMethodEnricher> OperationClassMethod { get; }
    }
}
