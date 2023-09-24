using System;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.SystemTextJson.Internal
{
    internal class DiscriminatorConverterTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiSchema, SystemTextJsonGeneratorCategory>
    {
        private readonly GenerationContext _context;
        private readonly IJsonSerializationNamespace _jsonSerializationNamespace;
        private readonly IRootNamespace _rootNamespace;

        public DiscriminatorConverterTypeGeneratorFactory(GenerationContext context, IJsonSerializationNamespace jsonSerializationNamespace,
            IRootNamespace rootNamespace)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(jsonSerializationNamespace);
            ArgumentNullException.ThrowIfNull(rootNamespace);

            _context = context;
            _jsonSerializationNamespace = jsonSerializationNamespace;
            _rootNamespace = rootNamespace;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiSchema> element, ITypeGenerator? parent) =>
            new DiscriminatorConverterTypeGenerator(element, _context, parent, _jsonSerializationNamespace, _rootNamespace);
    }
}
