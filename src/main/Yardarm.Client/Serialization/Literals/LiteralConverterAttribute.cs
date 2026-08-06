using System;
using System.Diagnostics.CodeAnalysis;

namespace RootNamespace.Serialization.Literals;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class LiteralConverterAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type) : Attribute
{
    public Type Type => type;

    public LiteralConverter CreateConverter() => (LiteralConverter)Activator.CreateInstance(type)!;
}
