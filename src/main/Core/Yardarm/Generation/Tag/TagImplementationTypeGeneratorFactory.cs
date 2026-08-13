using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Tag
{
    public class TagImplementationTypeGeneratorFactory : ITypeGeneratorFactory<IOpenApiTag>
    {
        private readonly IServiceProvider _serviceProvider;

        public TagImplementationTypeGeneratorFactory(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _serviceProvider = serviceProvider;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<IOpenApiTag> element, ITypeGenerator? parent) =>
            ActivatorUtilities.CreateInstance<TagImplementationTypeGenerator>(_serviceProvider, element);
    }
}
