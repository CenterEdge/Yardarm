using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
/// Serializes JSON Lines (newline-delimited JSON) bodies. Sequences are written and read as one JSON value per
/// line with <see cref="SerializeSequence{TSequence,TElement}"/> and
/// <see cref="DeserializeSequenceAsync{TSequence,TElement}"/>; other values are a single line.
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

    /// <summary>
    /// Serializes a single value as one JSON Lines record.
    /// </summary>
    public HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData = null) =>
        new JsonLinesContent<T[], T>([value], GetTypeInfo<T>(), _options, CreateMediaType(mediaType));

    public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData) =>
        DeserializeAsync<T>(content, serializationData, default);

    /// <summary>
    /// Deserializes a single JSON Lines record. Whitespace, including a trailing newline, is allowed after the
    /// record, but additional records are not.
    /// </summary>
    public async ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        Stream stream = await ReadAsStreamAsync(content, cancellationToken).ConfigureAwait(false);

        return (await JsonSerializer.DeserializeAsync(stream, GetTypeInfo<T>(), cancellationToken)
            .ConfigureAwait(false))!;
    }

    /// <summary>
    /// Serializes each element of a sequence as a JSON Lines record.
    /// </summary>
    public HttpContent SerializeSequence<TSequence, TElement>(TSequence value, string mediaType,
        ISerializationData? serializationData = null)
        where TSequence : IEnumerable<TElement> =>
        new JsonLinesContent<TSequence, TElement>(value, GetTypeInfo<TElement>(), _options, CreateMediaType(mediaType));

    /// <summary>
    /// Deserializes each JSON Lines record as an element of a sequence.
    /// </summary>
    /// <exception cref="NotSupportedException"><typeparamref name="TSequence"/> is not
    /// <see cref="List{T}"/>, an array or <see cref="IEnumerable{T}"/> of <typeparamref name="TElement"/>.</exception>
    public ValueTask<TSequence> DeserializeSequenceAsync<TSequence, TElement>(HttpContent content,
        ISerializationData? serializationData = null, CancellationToken cancellationToken = default)
        where TSequence : IEnumerable<TElement>
    {
        ArgumentNullException.ThrowIfNull(content);

        if (typeof(TSequence) != typeof(List<TElement>)
            && typeof(TSequence) != typeof(TElement[])
            && typeof(TSequence) != typeof(IEnumerable<TElement>))
        {
            throw new NotSupportedException(
                $"JSON Lines deserialization of {typeof(TSequence)} is not supported. Use List<T>, T[] or IEnumerable<T>.");
        }

        return DeserializeSequenceCoreAsync<TSequence, TElement>(content, cancellationToken);
    }

    private async ValueTask<TSequence> DeserializeSequenceCoreAsync<TSequence, TElement>(HttpContent content,
        CancellationToken cancellationToken)
        where TSequence : IEnumerable<TElement>
    {
        Stream stream = await ReadAsStreamAsync(content, cancellationToken).ConfigureAwait(false);

        IAsyncEnumerable<TElement?> elements = JsonSerializer.DeserializeAsyncEnumerable(stream,
            GetTypeInfo<TElement>(), topLevelValues: true, cancellationToken);

        object result = typeof(TSequence) == typeof(TElement[])
            ? await elements.ToArrayAsync(cancellationToken).ConfigureAwait(false)
            : await elements.ToListAsync(cancellationToken).ConfigureAwait(false);

        return (TSequence)result;
    }

    private JsonTypeInfo<T> GetTypeInfo<T>() => (JsonTypeInfo<T>)_options.GetTypeInfo(typeof(T));

    private static MediaTypeHeaderValue CreateMediaType(string mediaType) =>
        new(mediaType) { CharSet = Encoding.UTF8.WebName };

    private static Task<Stream> ReadAsStreamAsync(HttpContent content, CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        return content.ReadAsStreamAsync(cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return content.ReadAsStreamAsync();
#endif
    }

    /// <summary>
    /// Writes each element of a sequence as a JSON Lines record as the content is sent.
    /// </summary>
    private sealed class JsonLinesContent<TSequence, TElement> : HttpContent
        where TSequence : IEnumerable<TElement>
    {
        private static readonly byte[] s_newLine = [(byte)'\n'];

        private readonly TSequence _value;
        private readonly JsonTypeInfo<TElement> _typeInfo;
        private readonly JsonWriterOptions _writerOptions;

        public JsonLinesContent(TSequence value, JsonTypeInfo<TElement> typeInfo, JsonSerializerOptions options,
            MediaTypeHeaderValue mediaType)
        {
            _value = value;
            _typeInfo = typeInfo;

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
            if (_value is null)
            {
                return;
            }

            using var writer = new Utf8JsonWriter(stream, _writerOptions);

            foreach (TElement item in _value)
            {
                JsonSerializer.Serialize(writer, item, _typeInfo);
                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                writer.Reset();

#if NET5_0_OR_GREATER
                await stream.WriteAsync(s_newLine, cancellationToken).ConfigureAwait(false);
#else
                await stream.WriteAsync(s_newLine, 0, s_newLine.Length, cancellationToken).ConfigureAwait(false);
#endif
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
