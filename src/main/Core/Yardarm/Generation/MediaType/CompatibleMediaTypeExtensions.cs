using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Helpers;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType;

/// <summary>
/// Finds the media types of a response that the generated response class can read with the same body type as the
/// media type chosen by <see cref="IMediaTypeSelector"/>. For example, a JSON array of <c>T</c> and a JSON Lines
/// <c>itemSchema</c> of <c>T</c> both produce <c>List&lt;T&gt;</c>.
/// </summary>
internal static class CompatibleMediaTypeExtensions
{
    extension(ILocatedOpenApiElement<IOpenApiResponse> response)
    {
        /// <summary>
        /// Gets the media types with a serializer (quality greater than zero) whose body type matches the body type
        /// of the media type selected for the response, along with their serializer quality.
        /// </summary>
        public IEnumerable<(ILocatedOpenApiElement<IOpenApiMediaType> MediaType, double Quality)> GetCompatibleMediaTypes(
            IMediaTypeSelector mediaTypeSelector,
            ISerializerSelector serializerSelector,
            ITypeGeneratorRegistry typeGeneratorRegistry)
        {
            TypeSyntax? selectedBodyType = mediaTypeSelector.Select(response)?.GetBodyType(typeGeneratorRegistry);
            if (selectedBodyType is null)
            {
                return [];
            }

            return response.GetMediaTypes()
                .Select(p => (MediaType: p, Quality: serializerSelector.Select(p)?.Quality ?? 0.0))
                .Where(p => p.Quality > 0
                    && p.MediaType.GetBodyType(typeGeneratorRegistry) is { } bodyType
                    && bodyType.IsEquivalentTo(selectedBodyType));
        }
    }
}
