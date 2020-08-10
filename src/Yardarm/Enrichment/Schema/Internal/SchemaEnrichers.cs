using System.Collections.Generic;
using System.Linq;

namespace Yardarm.Enrichment.Schema.Internal
{
    internal class SchemaEnrichers : ISchemaEnrichers
    {
        public IList<ISchemaClassEnricher> Class { get; }
        public IList<ISchemaInterfaceEnricher> Interface { get; }
        public IList<ISchemaPropertyEnricher> Property { get; }
        public IList<ISchemaEnumEnricher> Enum { get; }
        public IList<ISchemaEnumMemberEnricher> EnumMember { get; }

        public SchemaEnrichers(
            IEnumerable<ISchemaClassEnricher> classEnrichers,
            IEnumerable<ISchemaInterfaceEnricher> interfaceEnrichers,
            IEnumerable<ISchemaPropertyEnricher> propertyEnrichers,
            IEnumerable<ISchemaEnumEnricher> enumEnrichers,
            IEnumerable<ISchemaEnumMemberEnricher> enumMemberEnrichers)
        {
            Class = classEnrichers.ToArray();
            Interface = interfaceEnrichers.ToArray();
            Property = propertyEnrichers.ToArray();
            Enum = enumEnrichers.ToArray();
            EnumMember = enumMemberEnrichers.ToArray();
        }
    }
}
