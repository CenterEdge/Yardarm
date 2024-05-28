using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace RootNamespace.Serialization.Json
{
    /// <summary>
    /// Variant of <see cref="JsonStringEnumConverter{TEnum}"/> that uses the <see cref="EnumMemberAttribute"/>
    /// for the enum member names.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    internal sealed class JsonNamedStringEnumConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>()
        : JsonStringEnumConverter<TEnum>(EnumMemberNamingPolicy<TEnum>.Instance)
        where TEnum : struct, Enum;
}
