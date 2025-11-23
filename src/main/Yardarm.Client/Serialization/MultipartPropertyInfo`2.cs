using System;
using System.Net.Http;

namespace RootNamespace.Serialization;

/// <summary>
/// Defines how to serialize multipart/form-data properties.
/// </summary>
/// <typeparam name="T">Type of schema to be serialized.</typeparam>
/// <typeparam name="TProperty">Type of the property to be serialized.</typeparam>
internal sealed class MultipartPropertyInfo<T, TProperty> : MultipartPropertyInfo<T>
{
    private readonly Func<T, TProperty> _propertyGetter;

    public MultipartPropertyInfo(Func<T, TProperty> propertyGetter,
        Func<T, MultipartFieldDetails?> detailsGetter,
        string propertyName,
        params string[] mediaTypes)
        : base(detailsGetter, propertyName, mediaTypes)
    {
        ArgumentNullException.ThrowIfNull(propertyGetter);

        _propertyGetter = propertyGetter;
    }

    protected override HttpContent Serialize(ITypeSerializerRegistry typeSerializerRegistry,
        string mediaType, T value) =>
        typeSerializerRegistry.Serialize(_propertyGetter(value), mediaType);
}
