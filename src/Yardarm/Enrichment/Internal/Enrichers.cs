using System.Collections.Generic;
using System.Linq;

namespace Yardarm.Enrichment.Internal
{
    internal class Enrichers : IEnrichers
    {
        public IList<ISchemaClassEnricher> ClassEnrichers { get; }
        public IList<ISchemaInterfaceEnricher> InterfaceEnrichers { get; }
        public IList<IOperationMethodEnricher> OperationMethodEnrichers { get; }
        public IList<IPropertyEnricher> PropertyEnrichers { get; }
        public IList<IEnumEnricher> EnumEnrichers { get; }
        public IList<IEnumMemberEnricher> EnumMemberEnrichers { get; }

        public Enrichers(
            IEnumerable<ISchemaClassEnricher> classEnrichers,
            IEnumerable<ISchemaInterfaceEnricher> interfaceEnrichers,
            IEnumerable<IOperationMethodEnricher> operationMethodEnrichers,
            IEnumerable<IPropertyEnricher> propertyEnrichers,
            IEnumerable<IEnumEnricher> enumEnrichers,
            IEnumerable<IEnumMemberEnricher> enumMemberEnrichers)
        {
            ClassEnrichers = classEnrichers.ToArray();
            InterfaceEnrichers = interfaceEnrichers.ToArray();
            OperationMethodEnrichers = operationMethodEnrichers.ToArray();
            PropertyEnrichers = propertyEnrichers.ToArray();
            EnumEnrichers = enumEnrichers.ToArray();
            EnumMemberEnrichers = enumMemberEnrichers.ToArray();
        }
    }
}
