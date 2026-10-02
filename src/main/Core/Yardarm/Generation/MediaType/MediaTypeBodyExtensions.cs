using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Helpers;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType;

/// <summary>
/// Resolves the body type of request and response media types.
/// </summary>
public static class MediaTypeBodyExtensions
{
    extension(ILocatedOpenApiElement<IOpenApiMediaType> mediaType)
    {
        /// <summary>
        /// Gets the schema whose type generator emits nested body models. For media types with an
        /// <c>itemSchema</c>, such as JSON Lines, this is the item schema. Otherwise it is the <c>schema</c>.
        /// </summary>
        public ILocatedOpenApiElement<IOpenApiSchema>? GetBodySchema() =>
            mediaType.GetItemSchema() ?? mediaType.GetSchema();

        /// <summary>
        /// Gets the C# type of the body. For media types with an <c>itemSchema</c>, such as JSON Lines,
        /// this is <see cref="System.Collections.Generic.List{T}"/> of the item type, the same type an
        /// array schema produces. Otherwise it is the type of the <c>schema</c>.
        /// </summary>
        /// <returns>The body type, or <c>null</c> if the media type has no schema.</returns>
        public TypeSyntax? GetBodyType(ITypeGeneratorRegistry typeGeneratorRegistry)
        {
            if (mediaType.GetItemSchema() is { } itemSchema)
            {
                return WellKnownTypes.System.Collections.Generic.ListT.Name(
                    typeGeneratorRegistry.Get(itemSchema).TypeInfo.Name);
            }

            return mediaType.GetSchema() is { } schema
                ? typeGeneratorRegistry.Get(schema).TypeInfo.Name
                : null;
        }
    }
}
