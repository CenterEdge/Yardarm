using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using RootNamespace.Internal;

namespace RootNamespace.Serialization.Json;

/// <summary>
/// Abstract base type for a JSON converter which handles type discrimination.
/// </summary>
/// <typeparam name="T">Base type of the polymorphic schema.</typeparam>
public abstract class JsonDiscriminatedObjectConverter<T> : JsonConverter<T>
    where T : class
{
    /// <summary>
    /// Defines how an unknown discriminator value is handled during deserialization.
    /// </summary>
    public UnknownDiscriminatorHandling UnknownDiscriminatorHandling
    {
        get;
        set
        {
            InvalidEnumArgumentException.ThrowIfNotDefined(value);

            field = value;
        }
    }

    /// <summary>
    /// Handles unknown discriminator values based on <see cref="UnknownDiscriminatorHandling"/>.
    /// </summary>
    /// <param name="reader">Reader ready to deserialize the object.</param>
    /// <param name="discriminator">Unknown discriminator value.</param>
    /// <returns>A deserialized object or <see langword="null"/>.</returns>
    /// <remarks>Should advance the <paramref name="reader"/> unless an exception is thrown.</remarks>
    /// <exception cref="JsonException">Unrecognized type discriminator.</exception>
    protected T? HandleUnknownDiscriminator(ref Utf8JsonReader reader, string? discriminator)
    {
        if (UnknownDiscriminatorHandling == UnknownDiscriminatorHandling.ReturnNull)
        {
            reader.Skip();
        }
        else
        {
            ThrowJsonException(discriminator);
        }

        return null;
    }

    [DoesNotReturn]
    private static void ThrowJsonException(string? discriminator) =>
        throw new JsonException($"Unrecognized type discriminator '{discriminator}'.");
}
