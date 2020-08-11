using System.Collections.Generic;

namespace Yardarm.Enrichment.Requests
{
    public interface IRequestEnrichers
    {
        IList<IOperationClassMethodEnricher> OperationClassMethod { get; }
        IList<IOperationInterfaceMethodEnricher> OperationInterfaceMethod { get; }
        IList<IOperationParameterPropertyEnricher> OperationParameterProperty { get; }
    }
}
