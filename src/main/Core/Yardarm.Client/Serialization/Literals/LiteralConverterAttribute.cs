using System;
using System.Diagnostics.CodeAnalysis;

namespace RootNamespace.Serialization.Literals;

/// <summary>
/// Specifies a literal converter to use for a type when serializing and deserializing literals.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
internal class LiteralConverterAttribute : Attribute
{
    /// <summary>
    /// Gets the type of the literal converter to use for the type this attribute is applied to.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type ConverterType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LiteralConverterAttribute"/> class with the specified converter type.
    /// </summary>
    /// <param name="converterType">The type of the literal converter to use. The type must be a <see cref="LiteralConverter"/> for the type this attribute is applied to and have a public parameterless constructor.</param>
    public LiteralConverterAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type converterType)
    {
        ArgumentNullException.ThrowIfNull(converterType);

        ConverterType = converterType;
    }

    /// <summary>
    /// Create a new instance of the <see cref="LiteralConverter"/> specified by <see cref="ConverterType"/>.
    /// </summary>
    /// <returns>A new instance of the specified <see cref="LiteralConverter"/>.</returns>
    /// <example cref="InvalidOperationException">The specified <see cref="ConverterType"/> is not a valid <see cref="LiteralConverter"/>.</example>
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
}
