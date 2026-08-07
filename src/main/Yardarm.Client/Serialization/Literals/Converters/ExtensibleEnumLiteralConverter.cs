using System;
using System.Diagnostics.CodeAnalysis;
using RootNamespace.Internal;
using RootNamespace.Models;

namespace RootNamespace.Serialization.Literals.Converters;

internal sealed class ExtensibleEnumLiteralConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
    : ValueTypeLiteralConverter<T>
    where T : struct, IExtensibleEnum<T>
{
    protected override T ReadCore(string value, string? format) => ExtensibleEnum.Create<T>(value);

    public override string Write(T value, string? format) => value.Value;

#if NET6_0_OR_GREATER

    public override bool TryWrite(T value, ReadOnlySpan<char> format, Span<char> destination, out int charsWritten)
    {
        string stringValue = value.Value ?? "";

        if (stringValue.TryCopyTo(destination))
        {
            charsWritten = stringValue.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

#endif
}
