using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Tag
{
    public class TagTypeGeneratorFactory : ITypeGeneratorFactory<IOpenApiTag>
    {
        private readonly IServiceProvider _serviceProvider;

        public TagTypeGeneratorFactory(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _serviceProvider = serviceProvider;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<IOpenApiTag> element, ITypeGenerator? parent) =>
            ActivatorUtilities.CreateInstance<TagTypeGenerator>(_serviceProvider, element);
    }
}
