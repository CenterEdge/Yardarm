using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RootNamespace.Serialization
{
    public class MultipartFormDataSerializer : ITypeSerializer
    {
        public HttpContent Serialize<T>(T value, string mediaType)
        {
            var content = new MultipartFormDataContent();

            // TODO: Complete this :)

            return content;
        }

        public ValueTask<T> DeserializeAsync<T>(HttpContent content) => throw new NotImplementedException();
    }
}
