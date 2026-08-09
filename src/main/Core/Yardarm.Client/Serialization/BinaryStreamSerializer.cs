using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace RootNamespace.Serialization;

public class BinaryStreamSerializer : ITypeSerializer
{
    private const string UnsupportedTypeMessage =
        $"{nameof(BinaryStreamSerializer)} only supports byte[] and Stream properties.";

    public static string[] SupportedMediaTypes => [MediaTypeNames.Application.Octet];

    public static Type[] SupportedSchemaTypes =>
    [
        typeof(Stream),
        typeof(byte[])
    ];

    public HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData)
    {
        HttpContent content = value switch
        {
            null => new ByteArrayContent([]),
            byte[] byteArray => new ByteArrayContent(byteArray),
            Stream stream => new StreamContent(stream),
            _ => throw new InvalidOperationException(UnsupportedTypeMessage)
        };

        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

        return content;
    }

    public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData) =>
        DeserializeAsync<T>(content, serializationData, default);

    public async ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null,
        // ReSharper disable once MethodOverloadWithOptionalParameter
        CancellationToken cancellationToken = default)
    {
        if (typeof(T) == typeof(byte[]))
        {
#if NET5_0_OR_GREATER
            return (T)(object)await content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
            cancellationToken.ThrowIfCancellationRequested();
            return (T)(object)await content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
        }

        if (typeof(Stream).IsAssignableFrom(typeof(T)))
        {
#if NET5_0_OR_GREATER
            return (T)(object)await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
            cancellationToken.ThrowIfCancellationRequested();
            return (T)(object)await content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
        }

        ThrowHelper.ThrowInvalidOperationException(UnsupportedTypeMessage);
        return default!; // unreachable
    }
}
