using System;

namespace RootNamespace.Models;

/// <summary>
/// String-based enumeration that is extensible and may contain any string value, not just well-known values.
/// </summary>
internal interface IExtensibleEnum<TSelf> : IEquatable<TSelf>
{
    /// <summary>
    /// Gets the value of the extensible enumeration.
    /// </summary>
    public string Value { get; }

#if NET7_0_OR_GREATER
    /// <summary>
    /// Creates a new instance of the extensible enumeration with the specified value.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> may not be null.</exception>
#endif
}
