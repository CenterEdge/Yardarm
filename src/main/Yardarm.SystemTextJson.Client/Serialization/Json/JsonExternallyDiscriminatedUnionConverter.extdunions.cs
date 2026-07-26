using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using RootNamespace.Internal;
using RootNamespace.Models;

namespace RootNamespace.Serialization.Json;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for property-discriminated unions that implement <see cref="IUnion"/>.
/// Constructors must be annotated with <see cref="UnionCaseNameAttribute"/>.
/// </summary>
/// <typeparam name="T">Type of the union.</typeparam>
internal sealed class JsonExternallyDiscriminatedUnionConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
    : JsonConverter<T>
    where T : IUnion
{
    #region Statics

    private sealed class CaseInfo(string caseName, Type caseType, Func<object, T> factory)
    {
        public readonly string CaseName = caseName;
        public readonly Type CaseType = caseType;
        public readonly Func<object, T> Factory = factory;
    }

    private static readonly FrozenDictionary<string, CaseInfo> s_casesByName;
    private static readonly FrozenDictionary<Type, CaseInfo> s_casesByType;
    private static readonly Func<object, T>? s_unknownCaseFactory;

    static JsonExternallyDiscriminatedUnionConverter()
    {
        var casesByName = new Dictionary<string, CaseInfo>();
        var casesByType = new Dictionary<Type, CaseInfo>();

        foreach (ConstructorInfo constructor in typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance))
        {
            ParameterInfo[] parameters = constructor.GetParameters();
            if (parameters.Length == 1 && parameters[0] is { ParameterType: Type parameterType })
            {
                if (parameterType == typeof(UnknownCase))
                {
                    s_unknownCaseFactory = CreateCaseFactory(constructor, parameterType);
                }
                else
                {
                    var attribute = constructor.GetCustomAttribute<UnionCaseNameAttribute>();
                    if (attribute is null)
                    {
                        continue;
                    }

                    var caseInfo = new CaseInfo(attribute.CaseName, parameterType,
                        CreateCaseFactory(constructor, parameterType));

                    casesByName[attribute.CaseName] = caseInfo;
                    casesByType[parameterType] = caseInfo;
                }
            }
        }

        s_casesByName = casesByName.ToFrozenDictionary();
        s_casesByType = casesByType.ToFrozenDictionary();
    }

    private static Func<object, T> CreateCaseFactory(ConstructorInfo constructor, Type parameterType)
    {
        ArgumentNullException.ThrowIfNull(constructor);
        ArgumentNullException.ThrowIfNull(parameterType);

#if NETCOREAPP3_0_OR_GREATER
        if (RuntimeFeature.IsDynamicCodeCompiled)
        {
#endif
            // More performant option that is dynamically compiled to avoid using reflection on each invocation
            ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "value");

            NewExpression newExpression = Expression.New(
                constructor,
                Expression.Convert(parameterExpression, parameterType));

            return Expression
                .Lambda<Func<object, T>>(newExpression, parameterExpression)
                .Compile();
#if NETCOREAPP3_0_OR_GREATER
        }
        else
        {
            // Fallback for scenarios where JIT is not supported
            return (value) => (T)constructor.Invoke([value]);
        }
#endif
    }

    private static T CreateUnknownCase(string? caseName = null)
    {
        if (s_unknownCaseFactory is null)
        {
            JsonDiscriminatedUnionConverter.ThrowUnknownUnionCase(typeof(T), caseName);
        }

        return s_unknownCaseFactory(UnknownCase.Value);
    }

#endregion

    public override bool HandleNull => false;

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
        }

        // Advance to the first property or EndObject token
        if (!reader.Read())
        {
            JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
        }

        if (reader.TokenType == JsonTokenType.EndObject)
        {
            // Empty object, always the unknown value
            return CreateUnknownCase();
        }

        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
        }

        string? propertyName = reader.GetString();

        T result;
        if (propertyName is not null && s_casesByName.TryGetValue(propertyName, out CaseInfo? caseInfo))
        {
            // Advance to the value token
            if (!reader.Read())
            {
                JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
            }

            JsonTypeInfo typeInfo = options.GetTypeInfo(caseInfo.CaseType);
            object? value = JsonSerializer.Deserialize(ref reader, typeInfo);
            if (value is null)
            {
                // Null is not a valid union case
                JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
            }

            result = caseInfo.Factory(value);

            // Advance past the value token
            if (!reader.Read())
            {
                JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
            }
        }
        else
        {
            result = CreateUnknownCase(propertyName);
        }

        while (reader.TokenType != JsonTokenType.EndObject)
        {
            // Skip any additional properties or the unknown variant
            reader.Skip();

            // Read to the next property or EndObject token
            if (!reader.Read())
            {
                JsonDiscriminatedUnionConverter.ThrowInvalidUnionJson(typeof(T));
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);

        object? innerValue = value.Value;
        if (innerValue is not null)
        {
            writer.WriteStartObject();

            if (innerValue is UnknownCase)
            {
                // Support round-trip serialization of the unknown case by serializing it as an empty object
                writer.WriteEndObject();
                return;
            }

            Type? innerType = innerValue.GetType();
            do
            {
                if (s_casesByType.TryGetValue(innerType, out CaseInfo? caseInfo))
                {
                    writer.WritePropertyName(caseInfo.CaseName);

                    JsonTypeInfo typeInfo = options.GetTypeInfo(caseInfo.CaseType);
                    JsonSerializer.Serialize(writer, innerValue, typeInfo);

                    writer.WriteEndObject();
                    return;
                }

                // Try the parent type to handle serializing inherited types, with the understanding that
                // it may not be deserializable without discriminators.
                innerType = innerType.BaseType;
            } while (innerType is not null);
        }

        // The union is invalid and contains a type for which it doesn't have a constructor.
        JsonDiscriminatedUnionConverter.ThrowUnknownUnionCaseType(typeof(T), innerValue?.GetType());
    }
}

internal static class JsonDiscriminatedUnionConverter
{
    [DoesNotReturn]
    public static void ThrowInvalidUnionJson(Type unionType)
        => throw new JsonException($"Invalid JSON for union type '{unionType.FullName}'.");

    [DoesNotReturn]
    public static void ThrowUnknownUnionCaseType(Type unionType, Type? caseType)
        => throw new JsonException(caseType is not null
            ? $"Union type '{unionType.FullName}' does not have a case with type '{caseType.FullName}'."
            : $"Union type '{unionType.FullName}' may not contain a null value.");

    [DoesNotReturn]
    public static void ThrowUnknownUnionCase(Type unionType, string? caseName)
        => throw new JsonException(caseName is not null
            ? $"Union type '{unionType.FullName}' does not have a case '{caseName}'."
            : $"Union type '{unionType.FullName}' does not have an unknown case.");
}
