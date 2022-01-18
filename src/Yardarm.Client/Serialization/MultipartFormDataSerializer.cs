using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace RootNamespace.Serialization
{
    public class MultipartFormDataSerializer : ITypeSerializer
    {
        public static string[] SupportedMediaTypes => new [] { "multipart/form-data" };

        private readonly ITypeSerializerRegistry _typeSerializerRegistry;

        public MultipartFormDataSerializer(ITypeSerializerRegistry typeSerializerRegistry)
        {
            _typeSerializerRegistry = typeSerializerRegistry ?? throw new ArgumentNullException(nameof(typeSerializerRegistry));
        }

        public HttpContent Serialize<T>(T value, string mediaType, MultipartFormDataSerializationData serializationData)
        {
            var content = new MultipartFormDataContent();

            foreach (PropertyInfo property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                SerializeProperty(content, serializationData, value, property);
            }

            return content;
        }

        HttpContent ITypeSerializer.Serialize<T>(T value, string mediaType, ISerializationData? serializationData)
        {
            if (!(serializationData is MultipartFormDataSerializationData multipartData))
            {
                throw new ArgumentException(
                    $"{nameof(serializationData)} must be of type {typeof(MultipartFormDataSerializationData).FullName}.");
            }

            return Serialize(value, mediaType, multipartData);
        }

        private void SerializeProperty(MultipartFormDataContent content,
            MultipartFormDataSerializationData serializationData, object? value, PropertyInfo property)
        {
            if (!serializationData.Encoding.TryGetValue(property.Name, out MultipartEncoding? encoding))
            {
                // All properties should have a provided encoding
                return;
            }

            // TODO: Complete this :)
            throw new NotImplementedException();
        }

        public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null) =>
            throw new NotImplementedException();
    }
}
