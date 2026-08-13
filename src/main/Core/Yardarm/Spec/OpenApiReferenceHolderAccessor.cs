using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OpenApi;
using Yardarm.Helpers;

namespace Yardarm.Spec;

internal static class OpenApiReferenceHolderAccessor
{
    private static readonly ConcurrentDictionary<Type, FallbackAccessor> s_fallbackAccessors = new();

    public static BaseOpenApiReference? GetReference(IOpenApiReferenceHolder holder)
    {
        ArgumentNullException.ThrowIfNull(holder);

        return holder switch
        {
            OpenApiCallbackReference reference => reference.Reference,
            OpenApiExampleReference reference => reference.Reference,
            OpenApiHeaderReference reference => reference.Reference,
            OpenApiLinkReference reference => reference.Reference,
            OpenApiMediaTypeReference reference => reference.Reference,
            OpenApiParameterReference reference => reference.Reference,
            OpenApiPathItemReference reference => reference.Reference,
            OpenApiRequestBodyReference reference => reference.Reference,
            OpenApiResponseReference reference => reference.Reference,
            OpenApiSchemaReference reference => reference.Reference,
            OpenApiSecuritySchemeReference reference => reference.Reference,
            OpenApiTagReference reference => reference.Reference,
            _ => s_fallbackAccessors.GetOrAdd(holder.GetType(), CreateFallbackAccessor).GetReference(holder)
        };
    }

    public static IOpenApiElement? GetTarget(IOpenApiReferenceHolder holder)
    {
        ArgumentNullException.ThrowIfNull(holder);

        return holder switch
        {
            OpenApiCallbackReference reference => reference.Target,
            OpenApiExampleReference reference => reference.Target,
            OpenApiHeaderReference reference => reference.Target,
            OpenApiLinkReference reference => reference.Target,
            OpenApiMediaTypeReference reference => reference.Target,
            OpenApiParameterReference reference => reference.Target,
            OpenApiPathItemReference reference => reference.Target,
            OpenApiRequestBodyReference reference => reference.Target,
            OpenApiResponseReference reference => reference.Target,
            OpenApiSchemaReference reference => reference.Target,
            OpenApiSecuritySchemeReference reference => reference.Target,
            OpenApiTagReference reference => reference.Target,
            _ => s_fallbackAccessors.GetOrAdd(holder.GetType(), CreateFallbackAccessor).GetTarget(holder)
        };
    }

    private static FallbackAccessor CreateFallbackAccessor(Type holderType) =>
        new(
            CreateGetter<BaseOpenApiReference>(holderType, "Reference"),
            CreateGetter<IOpenApiElement>(holderType, "Target"));

    private static AccessorProperty<T> CreateGetter<T>(Type holderType, string propertyName)
        where T : class
    {
        PropertyInfo? property = holderType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (property?.GetMethod is null || property.GetIndexParameters().Length != 0 ||
            !typeof(T).IsAssignableFrom(property.PropertyType))
        {
            return new AccessorProperty<T>(
                $"Reference holder type '{holderType.FullName}' must expose a public, non-indexed " +
                $"{propertyName} property assignable to '{typeof(T).FullName}'.");
        }

        var holder = Expression.Parameter(typeof(IOpenApiReferenceHolder), "holder");
        var propertyAccess = Expression.Property(Expression.Convert(holder, holderType), property);
        var convertedProperty = Expression.Convert(propertyAccess, typeof(T));

        return new AccessorProperty<T>(
            Expression.Lambda<Func<IOpenApiReferenceHolder, T?>>(convertedProperty, holder).Compile());
    }

    private sealed class FallbackAccessor(
        AccessorProperty<BaseOpenApiReference> reference,
        AccessorProperty<IOpenApiElement> target)
    {
        public BaseOpenApiReference? GetReference(IOpenApiReferenceHolder holder) => reference.Get(holder);

        public IOpenApiElement? GetTarget(IOpenApiReferenceHolder holder) => target.Get(holder);
    }

    private sealed class AccessorProperty<T>
        where T : class
    {
        private readonly Func<IOpenApiReferenceHolder, T?>? _getter;
        private readonly string? _errorMessage;

        public AccessorProperty(Func<IOpenApiReferenceHolder, T?> getter)
        {
            _getter = getter;
        }

        public AccessorProperty(string errorMessage)
        {
            _errorMessage = errorMessage;
        }

        public T? Get(IOpenApiReferenceHolder holder)
        {
            if (_getter is not null)
            {
                return _getter(holder);
            }

            ThrowHelpers.ThrowInvalidOperationException(_errorMessage);
            return default;
        }
    }
}
