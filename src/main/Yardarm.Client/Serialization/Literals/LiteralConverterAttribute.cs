using System;
using System.Diagnostics.CodeAnalysis;

namespace RootNamespace.Serialization.Literals;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
internal class LiteralConverterAttribute : Attribute
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type ConverterType { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type? NullableConverterType { get; }

    public LiteralConverterAttribute(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type converterType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type? nullableConverterType = null)
    {
        ArgumentNullException.ThrowIfNull(converterType);

        ConverterType = converterType;
        NullableConverterType = nullableConverterType;
    }

    public LiteralConverter CreateConverter()
    {
        object? obj = Activator.CreateInstance(ConverterType);

        if (obj is LiteralConverter converter)
        {
            return converter;
        }

        ThrowHelper.ThrowInvalidOperationException($"Type '{ConverterType.FullName}' is not a valid LiteralConverter.");
        return null!;
    }

    public LiteralConverter? CreateNullableConverter()
    {
        if (NullableConverterType is not Type nullableConverterType)
        {
            return null;
        }

        object? obj = Activator.CreateInstance(nullableConverterType, [CreateConverter()]);

        if (obj is LiteralConverter converter)
        {
            return converter;
        }

        ThrowHelper.ThrowInvalidOperationException($"Type '{nullableConverterType.FullName}' is not a valid LiteralConverter.");
        return null!;
    }
}
