using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;

namespace RootNamespace.Serialization
{
    public class BinaryStreamSerializer : ITypeSerializer
    {
        public static string[] SupportedMediaTypes => new[] {MediaTypeNames.Application.Octet};

        public static Type[] SupportedSchemaTypes => new[]
        {
            typeof(Stream),
            typeof(byte[])
        };

        public HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData)
        {
            HttpContent content = value switch
            {
                null => new ByteArrayContent(Array.Empty<byte>()),
                byte[] byteArray => new ByteArrayContent(byteArray),
                Stream stream => new StreamContent(stream),
                _ => throw new InvalidOperationException(
                    $"{nameof(BinaryStreamSerializer)} only supports byte[] and Stream properties.")
            };

            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            return content;
        }

        public async ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null)
        {
            if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)await content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }

            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)await content.ReadAsStreamAsync().ConfigureAwait(false);
            }

            throw new InvalidOperationException(
                $"{nameof(BinaryStreamSerializer)} only supports byte[] and Stream properties.");
        }
    }
}
