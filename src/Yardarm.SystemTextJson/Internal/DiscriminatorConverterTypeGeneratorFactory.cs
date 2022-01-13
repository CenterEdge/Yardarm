using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.SystemTextJson.Internal
{
    internal class DiscriminatorConverterTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiSchema, SystemTextJsonGeneratorCategory>
    {
        private readonly GenerationContext _context;
        private readonly IJsonSerializationNamespace _jsonSerializationNamespace;

        public DiscriminatorConverterTypeGeneratorFactory(GenerationContext context, IJsonSerializationNamespace jsonSerializationNamespace)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jsonSerializationNamespace = jsonSerializationNamespace ?? throw new ArgumentNullException(nameof(jsonSerializationNamespace));
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiSchema> element, ITypeGenerator? parent) =>
            new DiscriminatorConverterTypeGenerator(element, _context, parent, _jsonSerializationNamespace);
    }
}
