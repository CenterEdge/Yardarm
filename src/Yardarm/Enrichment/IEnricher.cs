using System;

namespace Yardarm.Enrichment
{
    public interface IEnricher
    {
        /// <summary>
        /// List of enricher types which must run before this enricher.
        /// </summary>
        public Type[] ExecuteAfter => Type.EmptyTypes;

        /// <summary>
        /// List of enricher types which must run after this enricher.
        /// </summary>
        public Type[] ExecuteBefore => Type.EmptyTypes;
    }
}
