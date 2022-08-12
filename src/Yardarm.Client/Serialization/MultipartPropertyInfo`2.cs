using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Net.Http;

namespace RootNamespace.Serialization
{
    /// <summary>
    /// Defines how to serialize multipart/form-data properties.
    /// </summary>
    /// <typeparam name="T">Type of schema to be serialized.</typeparam>
    /// <typeparam name="TProperty">Type of the property to be serialized.</typeparam>
    internal class MultipartPropertyInfo<T, TProperty> : MultipartPropertyInfo<T>
    {
        public MultipartPropertyInfo(Func<T, TProperty> propertyGetter,
            string propertyName,
            params string[] mediaTypes)
            : base(CreateContentSerializer(propertyGetter), propertyName, mediaTypes)
        {
        }

        private static Func<ITypeSerializer, string, T, HttpContent> CreateContentSerializer(
            Func<T, TProperty> propertyGetter)
        {
            HttpContent ContentSerializer(ITypeSerializer typeSerializer, string mediaType, T value)
            {
                return typeSerializer.Serialize(propertyGetter(value), mediaType);
            }

            return ContentSerializer;
        }
    }
}
