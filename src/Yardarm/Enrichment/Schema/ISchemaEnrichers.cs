using System.Collections.Generic;

namespace Yardarm.Enrichment.Schema
{
    public interface ISchemaEnrichers
    {
        IList<ISchemaClassEnricher> Class { get; }
        IList<ISchemaInterfaceEnricher> Interface { get; }
        IList<ISchemaPropertyEnricher> Property { get; }
        IList<ISchemaEnumEnricher> Enum { get; }
        IList<ISchemaEnumMemberEnricher> EnumMember { get; }
    }
}
