using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization.Json
{
    /// <summary>
    /// Adds special features to <see cref="AdditionalPropertiesDictionary{TValue}"/> for dynamically typed values.
    /// </summary>
    public sealed class DynamicAdditionalPropertiesDictionary : AdditionalPropertiesDictionary<dynamic>
    {
        public DynamicAdditionalPropertiesDictionary(IDictionary<string, JToken> dictionary) : base(dictionary)
        {
        }

        protected override JToken ToJToken(dynamic value) =>
            value is JToken jToken
                ? jToken
                : throw new ArgumentException("Dynamic types must be JToken types.", nameof(value));

        protected override dynamic ToValue(JToken token) => token;
    }

    /// <summary>
    /// Adds special features to <see cref="AdditionalPropertiesDictionary{TValue}"/> for dynamically typed values.
    /// </summary>
    public sealed class NullableDynamicAdditionalPropertiesDictionary : AdditionalPropertiesDictionary<dynamic?>
    {
        public NullableDynamicAdditionalPropertiesDictionary(IDictionary<string, JToken> dictionary) : base(dictionary)
        {
        }

        protected override JToken ToJToken(dynamic? value) =>
            value is JToken jToken
                ? jToken
                : value is null
                    ? JValue.CreateNull()
                    : throw new ArgumentException("Dynamic types must be JToken types.", nameof(value));

        protected override dynamic ToValue(JToken token) => token;
    }
}
