using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Yardarm.Client.Internal;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public class PlainTextSerializer : ITypeSerializer
    {
        public static string[] SupportedMediaTypes => new [] { MediaTypeNames.Text.Plain };

        public HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData = null) =>
            new StringContent(Serialize(value), Encoding.UTF8, mediaType);

        private static string Serialize<T>(T value) => value?.ToString() ?? "";

        public async ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null)
        {
            string value = await content.ReadAsStringAsync().ConfigureAwait(false);

            return Deserialize<T>(value);
        }

        private static T Deserialize<T>(string value)
        {
            if (typeof(T) == typeof(string) || typeof(T) == typeof(object))
            {
                return (T)(object)value;
            }

            ThrowHelper.ThrowInvalidOperationException($"Type '{typeof(T).FullName}' is not supported for deserialization by {nameof(PlainTextSerializer)}.");
            return default; // unreachable
        }
    }
}
