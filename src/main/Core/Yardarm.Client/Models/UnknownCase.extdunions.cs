namespace RootNamespace.Models;

/// <summary>
/// May be added to a union type to allow deserialization of unknown union cases to a placeholder type, rather than throwing an exception.
/// </summary>
/// <param name="CaseName">The name of the unknown case.</param>
/// <param name="Value">The value of the unknown case.</param>
/// <remarks>
/// <para>
/// The <paramref name="CaseName"/> and <paramref name="Value"/> properties are optional, and may be null if the unknown case does not have a name or value.
/// The type of the <paramref name="Value"/> property depends on the deserializer used.
/// </para>
/// </remarks>
public sealed record UnknownCase(string? CaseName, object? Value)
{
    public UnknownCase() : this(null, null) { }
}
