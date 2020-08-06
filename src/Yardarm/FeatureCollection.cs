using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Yardarm
{
    public class FeatureCollection : IFeatureCollection
    {
        private readonly IDictionary<Type, object> _features = new Dictionary<Type, object>();

        public object? this[Type key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                return _features.TryGetValue(key, out var feature) ? feature : null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null)
                {
                    _features.Remove(key);
                }
                else
                {
                    if (!key.IsInstanceOfType(value))
                    {
                        throw new ArgumentException("Invalid feature type", nameof(value));
                    }

                    _features[key] = value;
                }
            }
        }

        [return: MaybeNull]
        public TFeature Get<TFeature>() => (TFeature)this[typeof(TFeature)]!;

        public void Set<TFeature>(TFeature feature) => this[typeof(TFeature)] = feature;

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator() => _features.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
