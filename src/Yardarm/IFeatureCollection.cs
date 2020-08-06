using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Yardarm
{
    public interface IFeatureCollection : IEnumerable<KeyValuePair<Type, object>>
    {
        public object? this[Type key] { get; set; }

        [return: MaybeNull]
        TFeature Get<TFeature>();

        void Set<TFeature>(TFeature feature);
    }
}
