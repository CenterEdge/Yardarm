using System.Collections.Generic;
using System.Linq;

namespace Yardarm.Enrichment.Requests.Internal
{
    internal class RequestEnrichers : IRequestEnrichers
    {
        public IList<IRequestClassMethodEnricher> RequestClassMethod { get; }
        public IList<IRequestInterfaceMethodEnricher> RequestInterfaceMethod { get; }
        public IList<IRequestParameterPropertyEnricher> RequestParameterProperty { get; }

        public RequestEnrichers(
            IEnumerable<IRequestClassMethodEnricher> operationClassMethodEnrichers,
            IEnumerable<IRequestInterfaceMethodEnricher> operationInterfaceMethodEnrichers,
            IEnumerable<IRequestParameterPropertyEnricher> operationParameterPropertyEnrichers)
        {
            RequestClassMethod = operationClassMethodEnrichers.ToArray();
            RequestInterfaceMethod = operationInterfaceMethodEnrichers.ToArray();
            RequestParameterProperty = operationParameterPropertyEnrichers.ToArray();
        }
    }
}
