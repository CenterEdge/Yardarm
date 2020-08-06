using System.Collections.Generic;

namespace Yardarm.Enrichment
{
    public interface IEnrichers
    {
        IList<ISchemaClassEnricher> ClassEnrichers { get; }
        IList<ISchemaInterfaceEnricher> InterfaceEnrichers { get; }
        IList<IPropertyEnricher> PropertyEnrichers { get; }
        IList<IEnumEnricher> EnumEnrichers { get; }
        IList<IEnumMemberEnricher> EnumMemberEnrichers { get; }
    }
}
