using System.Collections.Generic;

namespace Yardarm.Enrichment.Requests
{
    public interface IRequestEnrichers
    {
        IList<IRequestClassMethodEnricher> RequestClassMethod { get; }
        IList<IRequestInterfaceMethodEnricher> RequestInterfaceMethod { get; }
        IList<IRequestParameterPropertyEnricher> RequestParameterProperty { get; }
    }
}
