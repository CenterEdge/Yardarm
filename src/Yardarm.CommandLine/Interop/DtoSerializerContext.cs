#if NET6_0_OR_GREATER

using System;
using System.Text.Json.Serialization;

#nullable enable

namespace Yardarm.CommandLine.Interop
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(AddItemDto))]
    internal partial class DtoSerializerContext : JsonSerializerContext
    {
    }
}

#endif
