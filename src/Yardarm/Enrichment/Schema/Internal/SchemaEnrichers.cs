using System.Collections.Generic;
using System.Linq;

namespace Yardarm.Enrichment.Schema.Internal
{
    internal class SchemaEnrichers : ISchemaEnrichers
    {
        public IList<ISchemaClassEnricher> Class { get; }
        public IList<ISchemaInterfaceEnricher> Interface { get; }
        public IList<ISchemaEnumEnricher> Enum { get; }
        public IList<ISchemaEnumMemberEnricher> EnumMember { get; }

        public SchemaEnrichers(
            IEnumerable<ISchemaClassEnricher> classEnrichers,
            IEnumerable<ISchemaInterfaceEnricher> interfaceEnrichers,
            IEnumerable<ISchemaEnumEnricher> enumEnrichers,
            IEnumerable<ISchemaEnumMemberEnricher> enumMemberEnrichers)
        {
            Class = classEnrichers.ToArray();
            Interface = interfaceEnrichers.ToArray();
            Enum = enumEnrichers.ToArray();
            EnumMember = enumMemberEnrichers.ToArray();
        }
    }
}
