using System.Text.Json;

namespace RootNamespace.Serialization.Json;

/// <summary>
/// Defines how an unknown discriminator value is handled during deserialization.
/// </summary>
public enum UnknownDiscriminatorHandling
{
    /// <summary>
    /// Throw a <see cref="JsonException"/>.
    /// </summary>
    ThrowException = 0,

    /// <summary>
    /// Return <see langword="null"/>.
    /// </summary>
    ReturnNull = 1,
}
