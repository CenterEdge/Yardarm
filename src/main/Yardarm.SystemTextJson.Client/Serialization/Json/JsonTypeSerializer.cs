using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization.Json
{
    public class JsonTypeSerializer : ITypeSerializer
    {
        internal const string SerializationUnreferencedCodeMessage = "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.";
        internal const string SerializationDynamicCodeMessage = "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext.";

        public static string[] SupportedMediaTypes => new []
        {
            "application/json",
            "application/json-patch+json",
            "text/json"
        };

        private readonly JsonSerializerOptions _options;

        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationDynamicCodeMessage)]
        public JsonTypeSerializer()
            : this(JsonSerializerOptions.Default)
        {
        }

        [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
        [RequiresDynamicCode(SerializationDynamicCodeMessage)]
        public JsonTypeSerializer(JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            options.MakeReadOnly(true);
            _options = options;
        }

        public JsonTypeSerializer(JsonSerializerContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _options = context.Options;
        }

        public HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData = null) =>
            JsonContent.Create(value, _options.GetTypeInfo(typeof(T)), new MediaTypeHeaderValue(mediaType) {CharSet = Encoding.UTF8.WebName});

        public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData) =>
            DeserializeAsync<T>(content, serializationData, default);

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
            Justification = "Incompatible constructors are marked with RequiresUnreferencedCode")]
        [UnconditionalSuppressMessage("Aot", "IL3050",
            Justification = "Incompatible constructors are marked with RequiresDynamicCode")]
        public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            CancellationToken cancellationToken = default) =>
            new(content.ReadFromJsonAsync<T>(_options, cancellationToken)!);
    }
}
