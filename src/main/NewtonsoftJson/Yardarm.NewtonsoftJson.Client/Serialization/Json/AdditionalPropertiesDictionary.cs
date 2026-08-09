using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization.Json
{
    public class AdditionalPropertiesDictionary<TValue> : IDictionary<string, TValue>
    {
        private readonly IDictionary<string, JToken> _dictionary;

        public AdditionalPropertiesDictionary(IDictionary<string, JToken> dictionary)
        {
            ArgumentNullException.ThrowIfNull(dictionary);

            _dictionary = dictionary;
        }

#pragma warning disable 8603
        protected virtual TValue ToValue(JToken token) =>
            token.ToObject<TValue>();
#pragma warning restore 8603

        protected virtual JToken ToJToken(TValue value) =>
            value == null
                ? JValue.CreateNull()
                : value switch
            {
                string str => new JValue(str),
                Uri uri => new JValue(uri),
                object obj when obj.GetType().IsValueType => new JValue(obj),
                JToken jToken => jToken,
                _ => JToken.FromObject(value)
            };

        /// <inheritdoc />
        public TValue this[string key]
        {
            get => ToValue(_dictionary[key]);
            set => _dictionary[key] = ToJToken(value);
        }

        /// <inheritdoc />
        public int Count => _dictionary.Count;

        /// <inheritdoc />
        public bool IsReadOnly => _dictionary.IsReadOnly;

        /// <inheritdoc />
        public ICollection<string> Keys => _dictionary.Keys;

        /// <inheritdoc />
        public ICollection<TValue> Values => _dictionary.Values.Select(ToValue).ToArray();

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() =>
            _dictionary
                .Select(p => new KeyValuePair<string, TValue>(p.Key, ToValue(p.Value)))
                .GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public void Add(KeyValuePair<string, TValue> item) => Add(item.Key, item.Value);

        /// <inheritdoc />
        public void Clear() => _dictionary.Clear();

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, TValue> item)
        {
            if (!TryGetValue(item.Key, out var value))
            {
                return false;
            }

            return EqualityComparer<TValue>.Default.Equals(item.Value, value);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);

            Array.Copy(this.ToArray(), 0, array, arrayIndex, Count);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, TValue> item)
        {
            if (!TryGetValue(item.Key, out var value))
            {
                return false;
            }

            if (!EqualityComparer<TValue>.Default.Equals(item.Value, value))
            {
                return false;
            }

            _dictionary.Remove(item.Key);
            return true;
        }

        /// <inheritdoc />
        public void Add(string key, TValue value) =>
            _dictionary.Add(key, ToJToken(value));

        /// <inheritdoc />
        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

        /// <inheritdoc />
        public bool Remove(string key) => _dictionary.Remove(key);

        /// <inheritdoc />
#pragma warning disable 8767
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
#pragma warning restore 8767
        {
            if (_dictionary.TryGetValue(key, out JToken? token))
            {
                value = ToValue(token);
                return true;
            }

            value = default;
            return false;
        }
    }
}
