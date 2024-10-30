using System;

namespace RootNamespace.Serialization.Literals;

/// <summary>
/// Non-generic base class for literal converters.
/// </summary>
public abstract class LiteralConverter
{
    /// <summary>
    /// Gets the type being converted by the current converter instance.
    /// </summary>
    public abstract Type? Type { get; }
}
