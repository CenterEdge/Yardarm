using System;
using System.Diagnostics.CodeAnalysis;

namespace RootNamespace.Serialization.Literals;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class LiteralConverterAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type) : Attribute
{
    public Type Type => type;

    public LiteralConverter CreateConverter()
    {
        object? obj = Activator.CreateInstance(type);

        if (obj is LiteralConverter converter)
        {
            return converter;
        }

        ThrowHelper.ThrowInvalidOperationException($"Type '{type.FullName}' is not a valid LiteralConverter.");
        return null!;
    }
}
