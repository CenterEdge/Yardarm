using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Response;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public class MediaTypeGeneratorFactory(IServiceProvider serviceProvider) : ITypeGeneratorFactory<OpenApiMediaType>
    {
        private static readonly ObjectFactory<RequestMediaTypeGenerator> Factory =
            ActivatorUtilities.CreateFactory<RequestMediaTypeGenerator>(
            [
                typeof(ILocatedOpenApiElement<OpenApiMediaType>),
                typeof(ITypeGenerator)
            ]);

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiMediaType> element, ITypeGenerator? parent)
        {
            ArgumentNullException.ThrowIfNull(parent);

            return parent switch
            {
                NoopTypeGenerator<OpenApiRequestBody> requestBodyParent => Factory(serviceProvider, [element, requestBodyParent]),
                ResponseTypeGenerator noop => new NoopTypeGenerator<OpenApiMediaType>(noop),
                _ => throw new ArgumentException("Unknown parent type for OpenApiMediaType", nameof(parent))
            };
        }
    }
}
