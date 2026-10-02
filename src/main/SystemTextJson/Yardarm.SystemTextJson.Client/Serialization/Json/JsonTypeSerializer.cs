using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
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
namespace RootNamespace.Serialization.Json;

public class JsonTypeSerializer : ISequenceTypeSerializer
{
    internal const string SerializationUnreferencedCodeMessage = "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.";
    internal const string SerializationDynamicCodeMessage = "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext.";

    public static string[] SupportedMediaTypes =>
    [
        "application/json",
        "application/json-patch+json",
        "text/json"
    ];

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

    private JsonTypeSerializer(JsonSerializerContext context, Action<JsonSerializerOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (configureOptions is null)
        {
            _options = context.Options;
        }
        else
        {
            var options = new JsonSerializerOptions(context.Options);
            configureOptions(options);
            options.MakeReadOnly();

            _options = options;
        }
    }

    /// <summary>
    /// Create a new <see cref="JsonTypeSerializer"/> instance with the default configuration.
    /// </summary>
    /// <param name="configureOptions">Optional callback to extend the default <see cref="JsonSerializerOptions"/>.</param>
    /// <returns>A new <see cref="JsonTypeSerializer"/> instance.</returns>
    public static JsonTypeSerializer CreateDefault(Action<JsonSerializerOptions>? configureOptions = null) =>
        new(ModelSerializerContext.Default, configureOptions);

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

    public HttpContent SerializeSequence<TItem>(IAsyncEnumerable<TItem> items, string mediaType,
        ISerializationData? serializationData = null)
    {
        ArgumentNullException.ThrowIfNull(items);

        return new JsonArraySequenceContent<TItem>(items, GetTypeInfo<TItem>(),
            new JsonWriterOptions
            {
                Encoder = _options.Encoder,
                Indented = _options.WriteIndented,
                MaxDepth = _options.MaxDepth
            },
            new MediaTypeHeaderValue(mediaType) { CharSet = Encoding.UTF8.WebName });
    }

    public IAsyncEnumerable<TItem> DeserializeSequenceAsync<TItem>(HttpContent content,
        ISerializationData? serializationData = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        return JsonSequenceReader.ReadAsync(content, GetTypeInfo<TItem>(), topLevelValues: false, cancellationToken);
    }

    private JsonTypeInfo<TItem> GetTypeInfo<TItem>() => (JsonTypeInfo<TItem>)_options.GetTypeInfo(typeof(TItem));

    /// <summary>
    /// Writes a sequence as a JSON array as it is enumerated.
    /// </summary>
    private sealed class JsonArraySequenceContent<TItem> : HttpContent
    {
        // Flush to the stream once this many bytes are buffered
        private const int FlushThreshold = 16 * 1024;

        private readonly IAsyncEnumerable<TItem> _items;
        private readonly JsonTypeInfo<TItem> _typeInfo;
        private readonly JsonWriterOptions _writerOptions;

        public JsonArraySequenceContent(IAsyncEnumerable<TItem> items, JsonTypeInfo<TItem> typeInfo,
            JsonWriterOptions writerOptions, MediaTypeHeaderValue mediaType)
        {
            _items = items;
            _typeInfo = typeInfo;
            _writerOptions = writerOptions;

            Headers.ContentType = mediaType;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            SerializeToStreamAsync(stream, CancellationToken.None);

#if NET5_0_OR_GREATER
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context,
            CancellationToken cancellationToken) =>
            SerializeToStreamAsync(stream, cancellationToken);
#endif

        private async Task SerializeToStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            using var writer = new Utf8JsonWriter(stream, _writerOptions);

            writer.WriteStartArray();

            await foreach (TItem item in _items.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                JsonSerializer.Serialize(writer, item, _typeInfo);

                if (writer.BytesPending >= FlushThreshold)
                {
                    await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            writer.WriteEndArray();
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
