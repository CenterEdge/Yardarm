using System;

namespace RootNamespace.Serialization.Literals;

/// <summary>
/// Interface for literal converters for value types that can create a converter for the nullable version of the type.
/// </summary>
internal interface IValueTypeLiteralConverter
{
    /// <summary>
    /// Creates a literal converter for the nullable version of this value type.
    /// </summary>
    /// <returns>A new literal converter for the nullable version of this type.</returns>
    /// <exception cref="InvalidOperationException">The type is already a nullable type.</exception>
    LiteralConverter CreateNullableConverter();
}
