using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Tag
{
    public class TagImplementationTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiTag, TagImplementationCategory>
    {
        private readonly IServiceProvider _serviceProvider;

        public TagImplementationTypeGeneratorFactory(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _serviceProvider = serviceProvider;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiTag> element, ITypeGenerator? parent) =>
            ActivatorUtilities.CreateInstance<TagImplementationTypeGenerator>(_serviceProvider, element);
    }
}
