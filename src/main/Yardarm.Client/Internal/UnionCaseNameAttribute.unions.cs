using System;

namespace RootNamespace.Internal;

/// <summary>
/// Annotates a union constructor with the name of the case it represents from the OpenAPI schema.
/// </summary>
/// <param name="caseName">Name of the property for this case.</param>
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
internal sealed class UnionCaseNameAttribute(string caseName) : Attribute
{
    /// <summary>
    /// Gets the name of the property for this case.
    /// </summary>
    public string CaseName { get; } = caseName;
}
