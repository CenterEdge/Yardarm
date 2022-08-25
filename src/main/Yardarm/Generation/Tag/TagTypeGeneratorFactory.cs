using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Tag
{
    public class TagTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiTag>
    {
        private readonly IServiceProvider _serviceProvider;

        public TagTypeGeneratorFactory(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _serviceProvider = serviceProvider;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiTag> element, ITypeGenerator? parent) =>
            ActivatorUtilities.CreateInstance<TagTypeGenerator>(_serviceProvider, element);
    }
}
