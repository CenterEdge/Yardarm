using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
    public static JsonLinesTypeSerializer CreateDefault(Action<JsonSerializerOptions>? configureOptions = null)
        => new(ModelSerializerContext.Default, configureOptions);

    /// <summary>
    /// Serializes a single value as one JSON Lines record.
    /// </summary>
    public HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData = null)
        // Write the value as a one record sequence so it has the same trailing newline as sequences
        => new JsonLinesContent<T[], T>([value], (JsonTypeInfo<T>)_options.GetTypeInfo(typeof(T)), _options,
            CreateMediaType(mediaType));

    public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData)
        => DeserializeAsync<T>(content, serializationData, CancellationToken.None);

    /// <summary>
    /// Deserializes a single JSON Lines record. Whitespace, including a trailing newline, is allowed after the
    /// record, but additional records are not.
    /// </summary>
    public async ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        // Transcodes to UTF-8 if the response declares another charset
        using Stream stream = await JsonContentStream.ReadAsUtf8StreamAsync(content, cancellationToken).ConfigureAwait(false);

        return (await JsonSerializer.DeserializeAsync(stream, (JsonTypeInfo<T>)_options.GetTypeInfo(typeof(T)),
            cancellationToken).ConfigureAwait(false))!;
    }

    /// <summary>
    /// Serializes each element of a sequence as a JSON Lines record.
    /// </summary>
    public HttpContent SerializeSequence<TSequence, TElement>(TSequence value, string mediaType,
        ISerializationData? serializationData = null)
        where TSequence : IEnumerable<TElement>
        => new JsonLinesContent<TSequence, TElement>(value,
            (JsonTypeInfo<TElement>)_options.GetTypeInfo(typeof(TElement)), _options, CreateMediaType(mediaType));

    /// <summary>
    /// Deserializes each JSON Lines record as an element of a sequence.
    /// </summary>
    /// <remarks>
    /// If <typeparamref name="TSequence"/> is <see cref="IAsyncEnumerable{T}"/> of <typeparamref name="TElement"/>, the
    /// result is returned immediately and the content isn't read until the first item is enumerated. Other supported
    /// sequence types are read completely before returning.
    /// </remarks>
    /// <exception cref="NotSupportedException"><typeparamref name="TSequence"/> is not
    /// <see cref="List{T}"/>, an array, <see cref="IEnumerable{T}"/> or <see cref="IAsyncEnumerable{T}"/> of
    /// <typeparamref name="TElement"/>.</exception>
    public ValueTask<TSequence> DeserializeSequenceAsync<TSequence, TElement>(HttpContent content,
        ISerializationData? serializationData = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (typeof(TSequence) == typeof(IAsyncEnumerable<TElement>))
        {
            return new((TSequence)(object)new JsonStreamingEnumerable<TElement>(content,
                (JsonTypeInfo<TElement>)_options.GetTypeInfo(typeof(TElement)), topLevelValues: true,
                cancellationToken));
        }

        if (typeof(TSequence) != typeof(List<TElement>)
            && typeof(TSequence) != typeof(TElement[])
            && typeof(TSequence) != typeof(IEnumerable<TElement>))
        {
            ThrowHelper.ThrowNotSupportedException(
                $"JSON Lines deserialization of {typeof(TSequence)} is not supported. Use List<T>, T[], IEnumerable<T> or IAsyncEnumerable<T>.");
        }

        return DeserializeSequenceCoreAsync<TSequence, TElement>(content, cancellationToken);
    }

    private async ValueTask<TSequence> DeserializeSequenceCoreAsync<TSequence, TElement>(HttpContent content,
        CancellationToken cancellationToken)
    {
        // Transcodes to UTF-8 if the response declares another charset
        using Stream stream = await JsonContentStream.ReadAsUtf8StreamAsync(content, cancellationToken).ConfigureAwait(false);

        IAsyncEnumerable<TElement?> elements = JsonSerializer.DeserializeAsyncEnumerable(stream,
            (JsonTypeInfo<TElement>)_options.GetTypeInfo(typeof(TElement)), topLevelValues: true, cancellationToken);

        object result = typeof(TSequence) == typeof(TElement[])
            ? await elements.ToArrayAsync(cancellationToken).ConfigureAwait(false)
            : await elements.ToListAsync(cancellationToken).ConfigureAwait(false);

        return (TSequence)result;
    }

    private static MediaTypeHeaderValue CreateMediaType(string mediaType)
        => new(mediaType) { CharSet = Encoding.UTF8.WebName };
}
