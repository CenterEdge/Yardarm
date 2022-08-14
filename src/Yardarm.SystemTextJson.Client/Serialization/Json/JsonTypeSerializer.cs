using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization.Json
{
    public class JsonTypeSerializer : ITypeSerializer
    {
        public static string[] SupportedMediaTypes => new []
        {
            "application/json",
            "application/json-patch+json",
            "text/json"
        };

        private readonly JsonSerializerOptions _options;

        public JsonTypeSerializer()
            : this(new JsonSerializerOptions())
        {
        }

        public JsonTypeSerializer(JsonSerializerOptions options)
        {
            _options = options;
        }

        public HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData = null) =>
            JsonContent.Create(value, new MediaTypeHeaderValue("application/json"), _options);

        public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null)
        {
            return new ValueTask<T>(content.ReadFromJsonAsync<T>(_options)!);
        }
    }
}
