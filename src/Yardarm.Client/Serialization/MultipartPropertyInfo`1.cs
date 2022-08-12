using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;

namespace RootNamespace.Serialization
{
    /// <summary>
    /// Defines how to serialize multipart/form-data properties.
    /// </summary>
    /// <typeparam name="T">Type of schema to be serialized.</typeparam>
    public abstract class MultipartPropertyInfo<T>
    {
        private readonly Func<ITypeSerializer, string, T, HttpContent> _contentSerializer;

        public string PropertyName { get; }

        public IReadOnlyCollection<string> MediaTypes { get; }

        protected MultipartPropertyInfo(Func<ITypeSerializer, string, T, HttpContent> contentSerializer,
            string propertyName,
            params string[] mediaTypes)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(propertyName);
            ArgumentNullException.ThrowIfNull(contentSerializer);
            ArgumentNullException.ThrowIfNull(mediaTypes);
#else
            if (propertyName is null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            if (contentSerializer is null)
            {
                throw new ArgumentNullException(nameof(contentSerializer));
            }
            if (mediaTypes is null)
            {
                throw new ArgumentNullException(nameof(mediaTypes));
            }
#endif

            PropertyName = propertyName;
            _contentSerializer = contentSerializer;
            MediaTypes = new ReadOnlyCollection<string>(mediaTypes);
        }

        public HttpContent Serialize(ITypeSerializerRegistry typeSerializerRegistry, T value)
        {
            string mediaType = MediaTypes.First();
            if (!typeSerializerRegistry.TryGet(mediaType, out ITypeSerializer? serializer))
            {
                throw new UnknownMediaTypeException(mediaType);
            }

            return _contentSerializer(serializer, mediaType, value);
        }

        public static MultipartPropertyInfo<T> Create<TProperty>(
            Func<T, TProperty> propertyGetter, string propertyName, params string[] mediaTypes) =>
            new MultipartPropertyInfo<T, TProperty>(propertyGetter, propertyName, mediaTypes);
    }
}
