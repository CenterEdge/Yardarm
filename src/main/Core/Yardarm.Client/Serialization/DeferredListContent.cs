using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization;

/// <summary>
/// Collects an <see cref="IAsyncEnumerable{T}"/> into a <see cref="List{T}"/> and serializes it with an
/// <see cref="ITypeSerializer"/> immediately before the content is sent. Used for serializers that do not
/// implement <see cref="ISequenceTypeSerializer"/>.
/// </summary>
/// <remarks>
/// Content headers are sent before the content, so the content type is set from the media type when this content is
/// created. Other headers the inner serializer would set, such as the charset or content length, are not sent.
/// </remarks>
internal sealed class DeferredListContent<TItem> : HttpContent
{
    private readonly ITypeSerializer _typeSerializer;
    private readonly IAsyncEnumerable<TItem> _items;
    private readonly string _mediaType;
    private readonly ISerializationData? _serializationData;

    private HttpContent? _innerContent;

    public DeferredListContent(ITypeSerializer typeSerializer, IAsyncEnumerable<TItem> items, string mediaType,
        ISerializationData? serializationData)
    {
        _typeSerializer = typeSerializer;
        _items = items;
        _mediaType = mediaType;
        _serializationData = serializationData;

        Headers.ContentType = new MediaTypeHeaderValue(mediaType);
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
        if (_innerContent is null)
        {
            List<TItem> items = await SequenceHelpers.ToListAsync(_items, cancellationToken).ConfigureAwait(false);

            _innerContent = _typeSerializer.Serialize(items, _mediaType, _serializationData);
        }

#if NET5_0_OR_GREATER
        await _innerContent.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
#else
        await _innerContent.CopyToAsync(stream).ConfigureAwait(false);
#endif
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _innerContent?.Dispose();
        }

        base.Dispose(disposing);
    }
}
