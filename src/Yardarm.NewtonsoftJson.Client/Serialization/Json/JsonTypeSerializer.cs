using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization.Json
{
    public class JsonTypeSerializer : ITypeSerializer
    {
        public static string[] SupportedMediaTypes => new []
        {
            "application/json",
            "text/json",
            "application/json-patch+json"
        };

        private readonly JsonSerializer _serializer;

        public JsonTypeSerializer()
            : this(new JsonSerializerSettings())
        {
        }

        public JsonTypeSerializer(JsonSerializerSettings settings)
        {
            _serializer = JsonSerializer.Create(settings);
        }

        public HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData = null)
        {
            var stream = new MemoryStream();
            try
            {
                using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);
                _serializer.Serialize(writer, value, typeof(T));
                writer.Flush();

                stream.Position = 0;
                return new StreamContent(stream)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue(mediaType) {CharSet = Encoding.UTF8.WebName},
                        ContentLength = stream.Length
                    }
                };
            }
            catch
            {
                // Be sure to cleanup the stream if there's an exception
                stream.Dispose();
                throw;
            }
        }

        public async ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null)
        {
            string str = await content.ReadAsStringAsync().ConfigureAwait(false);

            using var reader = new JsonTextReader(new StringReader(str));

            return _serializer.Deserialize<T>(reader)!;
        }
    }
}
