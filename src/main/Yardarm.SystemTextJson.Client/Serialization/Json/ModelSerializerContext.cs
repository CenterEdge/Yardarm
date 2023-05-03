#if FORTESTS

// This file is included only for local compilation of Yardarm.SystemTextJson.Client to support testing.
// It is not included in an output SDK from Yardarm, instead Yardarm.SystemTextJson adds its own version
// of a class with the same name to the compilation.

using System;
using System.Text.Json.Serialization;

namespace RootNamespace.Serialization.Json
{
    [JsonSerializable(typeof(FakeModel))]
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization)]
    public partial class ModelSerializerContext : JsonSerializerContext
    {

    }

    public class FakeModel { }
}
#endif
