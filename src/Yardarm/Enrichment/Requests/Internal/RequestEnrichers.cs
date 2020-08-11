using System.Collections.Generic;
using System.Linq;

namespace Yardarm.Enrichment.Requests.Internal
{
    internal class RequestEnrichers : IRequestEnrichers
    {
        public IList<IOperationClassMethodEnricher> OperationClassMethod { get; }
        public IList<IOperationInterfaceMethodEnricher> OperationInterfaceMethod { get; }
        public IList<IOperationParameterPropertyEnricher> OperationParameterProperty { get; }

        public RequestEnrichers(
            IEnumerable<IOperationClassMethodEnricher> operationClassMethodEnrichers,
            IEnumerable<IOperationInterfaceMethodEnricher> operationInterfaceMethodEnrichers,
            IEnumerable<IOperationParameterPropertyEnricher> operationParameterPropertyEnrichers)
        {
            OperationClassMethod = operationClassMethodEnrichers.ToArray();
            OperationInterfaceMethod = operationInterfaceMethodEnrichers.ToArray();
            OperationParameterProperty = operationParameterPropertyEnrichers.ToArray();
        }
    }
}
