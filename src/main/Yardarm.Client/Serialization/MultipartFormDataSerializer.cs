using System;
using System.Net.Http;
using System.Threading.Tasks;
using Yardarm.Client.Internal;

namespace RootNamespace.Serialization
{
    public class MultipartFormDataSerializer : ITypeSerializer
    {
        public static string[] SupportedMediaTypes => new [] { "multipart/form-data" };

        private readonly ITypeSerializerRegistry _typeSerializerRegistry;

        public MultipartFormDataSerializer(ITypeSerializerRegistry typeSerializerRegistry)
        {
            ThrowHelper.ThrowIfNull(typeSerializerRegistry);

            _typeSerializerRegistry = typeSerializerRegistry;
        }

        public HttpContent Serialize<T>(T value, string mediaType, MultipartFormDataSerializationData<T> serializationData)
        {
            var content = new MultipartFormDataContent();

            if (value is not null)
            {
                foreach (MultipartPropertyInfo<T> property in serializationData.Properties)
                {
                    HttpContent propertyContent = property.Serialize(_typeSerializerRegistry, value);

                    string? filename = property.GetDetails(value)?.Filename;
                    if (filename is null)
                    {
                        content.Add(propertyContent, property.PropertyName);
                    }
                    else
                    {
                        content.Add(propertyContent, property.PropertyName, filename);
                    }
                }
            }

            return content;
        }

        HttpContent ITypeSerializer.Serialize<T>(T value, string mediaType, ISerializationData? serializationData)
        {
            if (serializationData is not MultipartFormDataSerializationData<T> multipartData)
            {
                throw new ArgumentException(
                    $"{nameof(serializationData)} must be of type {typeof(MultipartFormDataSerializationData<T>).FullName}.");
            }

            return Serialize(value, mediaType, multipartData);
        }

        public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null) =>
            throw new NotImplementedException();
    }
}
