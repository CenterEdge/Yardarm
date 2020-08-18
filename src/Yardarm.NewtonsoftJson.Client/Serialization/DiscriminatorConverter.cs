using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public class DiscriminatorConverter : JsonConverter
    {
        private readonly string _propertyName;
        private readonly Type _interfaceType;
        private readonly IDictionary<string, Type> _mappings;

        public override bool CanRead => true;
        public override bool CanWrite => false;

        public DiscriminatorConverter(string propertyName, Type interfaceType, params object[] mappings)
            : this(propertyName, interfaceType, Pair(mappings))
        {
        }

        public DiscriminatorConverter(string propertyName, Type interfaceType, IEnumerable<KeyValuePair<string, Type>> mappings)
        {
            _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            _interfaceType = interfaceType ?? throw new ArgumentNullException(nameof(interfaceType));

            if (mappings == null)
            {
                throw new ArgumentNullException(nameof(mappings));
            }

            _mappings = new Dictionary<string, Type>();
            foreach (var mapping in mappings)
            {
                _mappings.Add(mapping);
            }
        }

        public override bool CanConvert(Type objectType) => objectType == _interfaceType;

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var json = JObject.Load(reader);
            var discriminator = json.Property(_propertyName)?.Value?.ToString();
            if (discriminator is null)
            {
                throw new JsonSerializationException($"Could not find discriminator attribute '{_propertyName}'.");
            }

            if (!_mappings.TryGetValue(discriminator, out var type))
            {
                throw new JsonSerializationException($"Could not find discriminator mapping '{discriminator}'.");
            }

            if (type != objectType)
            {
                var nextConverter = type.GetCustomAttribute<JsonConverterAttribute>();
                if (nextConverter != null)
                {
                    return serializer.Deserialize(json.CreateReader(), type);
                }
            }

            var result = Activator.CreateInstance(type);
            serializer.Populate(json.CreateReader(), result);
            return result;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<KeyValuePair<string, Type>> Pair(IEnumerable<object> items)
        {
            string? key = null;
            foreach (object item in items)
            {
                if (key is null)
                {
                    key = (string)item;
                }
                else
                {
                    yield return new KeyValuePair<string, Type>(key, (Type) item);
                    key = null;
                }
            }
        }
    }
}
