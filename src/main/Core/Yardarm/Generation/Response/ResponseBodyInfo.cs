using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Response;

/// <summary>
/// Describes the body of a response, as selected for deserialization.
/// </summary>
/// <param name="MediaType">The selected media type.</param>
/// <param name="BodyType">The C# type of the body.</param>
/// <param name="ItemType">
/// The C# type of each item for sequential bodies, otherwise <c>null</c>. Sequential bodies are deserialized with
/// <c>DeserializeSequenceAsync</c>.
/// </param>
/// <param name="IsStreaming">
/// If true, the body is an <c>IAsyncEnumerable&lt;T&gt;</c> of <paramref name="ItemType"/> which is read as it is enumerated.
/// </param>
public sealed record ResponseBodyInfo(
    ILocatedOpenApiElement<IOpenApiMediaType> MediaType,
    TypeSyntax BodyType,
    TypeSyntax? ItemType,
    bool IsStreaming);
