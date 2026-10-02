using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization.Json;

/// <summary>
/// Serializes JSON Lines (newline-delimited JSON) bodies. Collections are written and read as one JSON
/// value per line; other values are written and read as a single line.
/// </summary>
public class JsonLinesTypeSerializer : ITypeSerializer
{
    public static string[] SupportedMediaTypes =>
    [
        "application/jsonl",
        "application/x-ndjson"
    ];

    private readonly JsonSerializerOptions _options;

    [RequiresUnreferencedCode(JsonTypeSerializer.SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(JsonTypeSerializer.SerializationDynamicCodeMessage)]
    public JsonLinesTypeSerializer()
        : this(JsonSerializerOptions.Default)
    {
    }

    [RequiresUnreferencedCode(JsonTypeSerializer.SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(JsonTypeSerializer.SerializationDynamicCodeMessage)]
    public JsonLinesTypeSerializer(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.MakeReadOnly(true);
        _options = options;
    }

    public JsonLinesTypeSerializer(JsonSerializerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _options = context.Options;
    }

    private JsonLinesTypeSerializer(JsonSerializerContext context, Action<JsonSerializerOptions>? configureOptions = null)
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
    /// Create a new <see cref="JsonLinesTypeSerializer"/> instance with the default configuration.
    /// </summary>
    /// <param name="configureOptions">Optional callback to extend the default <see cref="JsonSerializerOptions"/>.</param>
    /// <returns>A new <see cref="JsonLinesTypeSerializer"/> instance.</returns>
    public static JsonLinesTypeSerializer CreateDefault(Action<JsonSerializerOptions>? configureOptions = null) =>
        new(ModelSerializerContext.Default, configureOptions);

    public HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData = null)
    {
        JsonTypeInfo typeInfo = _options.GetTypeInfo(typeof(T));

        return new JsonLinesContent(value, typeInfo, GetItemTypeInfo(typeInfo), _options,
            new MediaTypeHeaderValue(mediaType) { CharSet = Encoding.UTF8.WebName });
    }

    public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData) =>
        DeserializeAsync<T>(content, serializationData, default);

    public async ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

#if NET5_0_OR_GREATER
        byte[] utf8Json = await content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
        byte[] utf8Json = await content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif

        return Deserialize<T>(utf8Json, cancellationToken);
    }

    private T Deserialize<T>(ReadOnlySpan<byte> utf8Json, CancellationToken cancellationToken)
    {
        // Utf8JsonReader does not skip a byte order mark
        ReadOnlySpan<byte> utf8Bom = [0xEF, 0xBB, 0xBF];
        if (utf8Json.StartsWith(utf8Bom))
        {
            utf8Json = utf8Json.Slice(utf8Bom.Length);
        }

        var reader = new Utf8JsonReader(utf8Json, new JsonReaderOptions
        {
            AllowMultipleValues = true,
            AllowTrailingCommas = _options.AllowTrailingCommas,
            CommentHandling = _options.ReadCommentHandling,
            MaxDepth = _options.MaxDepth
        });

        JsonTypeInfo typeInfo = _options.GetTypeInfo(typeof(T));
        JsonTypeInfo? itemTypeInfo = GetItemTypeInfo(typeInfo);
        if (itemTypeInfo is null)
        {
            if (!reader.Read())
            {
                return default!;
            }

            return (T)JsonSerializer.Deserialize(ref reader, typeInfo)!;
        }

        if (typeInfo.CreateObject?.Invoke() is not IList list)
        {
            throw new NotSupportedException($"JSON Lines deserialization of {typeof(T)} requires a type that implements {nameof(IList)} with a default constructor.");
        }

        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            list.Add(JsonSerializer.Deserialize(ref reader, itemTypeInfo));
        }

        return (T)list;
    }

    private JsonTypeInfo? GetItemTypeInfo(JsonTypeInfo typeInfo) =>
        typeInfo is { Kind: JsonTypeInfoKind.Enumerable, ElementType: { } elementType }
            ? _options.GetTypeInfo(elementType)
            : null;

    private sealed class JsonLinesContent : HttpContent
    {
        private static readonly byte[] s_newLine = [(byte)'\n'];

        private readonly object? _value;
        private readonly JsonTypeInfo _typeInfo;
        private readonly JsonTypeInfo? _itemTypeInfo;
        private readonly JsonWriterOptions _writerOptions;

        public JsonLinesContent(object? value, JsonTypeInfo typeInfo, JsonTypeInfo? itemTypeInfo,
            JsonSerializerOptions options, MediaTypeHeaderValue mediaType)
        {
            _value = value;
            _typeInfo = typeInfo;
            _itemTypeInfo = itemTypeInfo;

            // Records must not span lines, so the writer is never indented regardless of the serializer options
            _writerOptions = new JsonWriterOptions
            {
                Encoder = options.Encoder,
                Indented = false,
                MaxDepth = options.MaxDepth
            };

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

            if (_itemTypeInfo is null)
            {
                await WriteLineAsync(stream, writer, _value, _typeInfo, cancellationToken).ConfigureAwait(false);
                return;
            }

            if (_value is IEnumerable items)
            {
                foreach (object? item in items)
                {
                    await WriteLineAsync(stream, writer, item, _itemTypeInfo, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static async Task WriteLineAsync(Stream stream, Utf8JsonWriter writer, object? value,
            JsonTypeInfo typeInfo, CancellationToken cancellationToken)
        {
            JsonSerializer.Serialize(writer, value, typeInfo);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            writer.Reset();

#if NET5_0_OR_GREATER
            await stream.WriteAsync(s_newLine, cancellationToken).ConfigureAwait(false);
#else
            await stream.WriteAsync(s_newLine, 0, s_newLine.Length, cancellationToken).ConfigureAwait(false);
#endif
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
