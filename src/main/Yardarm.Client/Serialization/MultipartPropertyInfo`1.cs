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
        private readonly Func<T, MultipartFieldDetails?> _detailsGetter;

        public string PropertyName { get; }

        public IReadOnlyCollection<string> MediaTypes { get; }

        protected MultipartPropertyInfo(Func<T, MultipartFieldDetails?> detailsGetter,
            string propertyName, params string[] mediaTypes)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(detailsGetter);
            ArgumentNullException.ThrowIfNull(propertyName);
            ArgumentNullException.ThrowIfNull(mediaTypes);
#else
            if (detailsGetter is null)
            {
                throw new ArgumentNullException(nameof(detailsGetter));
            }
            if (propertyName is null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            if (mediaTypes is null)
            {
                throw new ArgumentNullException(nameof(mediaTypes));
            }
#endif

            _detailsGetter = detailsGetter;
            PropertyName = propertyName;
            MediaTypes = new ReadOnlyCollection<string>(mediaTypes);
        }

        public HttpContent Serialize(ITypeSerializerRegistry typeSerializerRegistry, T value)
        {
            string mediaType = _detailsGetter(value)?.ContentType ?? MediaTypes.First();

            return Serialize(typeSerializerRegistry, mediaType, value);
        }

        protected abstract HttpContent Serialize(ITypeSerializerRegistry typeSerializerRegistry,
            string mediaType, T value);

        public MultipartFieldDetails? GetDetails(T value) => _detailsGetter(value);

        public static MultipartPropertyInfo<T> Create<TProperty>(
            Func<T, TProperty> propertyGetter, Func<T, MultipartFieldDetails?> detailsGetter,
            string propertyName, params string[] mediaTypes) =>
            new MultipartPropertyInfo<T, TProperty>(propertyGetter, detailsGetter, propertyName, mediaTypes);
    }
}
