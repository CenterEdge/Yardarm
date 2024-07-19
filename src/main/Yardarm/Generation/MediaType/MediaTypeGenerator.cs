using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public class MediaTypeGenerator(
        OpenApiDocument document,
        ITypeGeneratorRegistry<OpenApiMediaType> mediaTypeGeneratorRegistry,
        ISerializerSelector serializerSelector,
        IOperationNameProvider operationNameProvider) : ISyntaxTreeGenerator
    {
        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in GetMediaTypes()
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        private IEnumerable<ILocatedOpenApiElement<OpenApiMediaType>> GetMediaTypes()
        {
            foreach (var requestBody in GetRequestBodies())
            {
                var mediaTypes = requestBody.GetMediaTypes()
                    .Select(mediaType => (mediaType, descriptor: serializerSelector.Select(mediaType)))
                    .Where(p => p.descriptor is not null)
                    .Select(p => (p.mediaType, descriptor: p.descriptor.GetValueOrDefault()));

                // If multiple content types on the same request body match on the same serializer name, i.e. "application/json" and "text/json"
                // then we pick the one that has the highest priority. This allows the following:
                //   A) we can support alternate content types on requests like "text/json" when it is the only content type present
                //   B) if BOTH the primary and alternate are present, we pick the primary and don't generate a type for the secondary,
                //      without generating naming conflicts.
                //   C) we don't generate HttpContent request bodies for known content types like "text/json"

                var groupedByName = mediaTypes.GroupBy(p => p.descriptor.Descriptor.NameSegment);
                foreach (var group in groupedByName)
                {
                    yield return group
                        .OrderByDescending(p => p.descriptor.Quality)
                        .Select(p => p.mediaType)
                        .First();
                }
            }
        }

        private IEnumerable<ILocatedOpenApiElement<OpenApiRequestBody>> GetRequestBodies() =>
            document.Paths.ToLocatedElements()
                .GetOperations()
                .WhereOperationHasName(operationNameProvider)
                .GetRequestBodies();

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiMediaType> mediaType) =>
            mediaTypeGeneratorRegistry.Get(mediaType).GenerateSyntaxTree();
    }
}
