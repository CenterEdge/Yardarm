using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization.Json;

/// <summary>
/// Writes each element of a sequence as a JSON Lines record as the content is sent.
/// </summary>
internal sealed class JsonLinesContent<TSequence, TElement> : HttpContent
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
            MaxDepth = options.MaxDepth,
#if !DEBUG
            // The serializer writes valid JSON, skip validating it outside of debug builds
            SkipValidation = true
#endif
        };

        Headers.ContentType = mediaType;
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        => SerializeToStreamAsync(stream, CancellationToken.None);

#if NET5_0_OR_GREATER
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context,
        CancellationToken cancellationToken)
        => SerializeToStreamAsync(stream, cancellationToken);
#endif

    private async Task SerializeToStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (_value is null)
        {
            return;
        }

        using var writer = new Utf8JsonWriter(stream, _writerOptions);

        bool first = true;
        foreach (TElement item in _value)
        {
            if (!first)
            {
                // Each record is a new root value
                writer.Reset();
            }

            first = false;

            JsonSerializer.Serialize(writer, item, _typeInfo);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

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
