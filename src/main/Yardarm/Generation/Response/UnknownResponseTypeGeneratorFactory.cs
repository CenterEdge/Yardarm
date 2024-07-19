using System;
using Microsoft.Extensions.DependencyInjection;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class UnknownResponseTypeGeneratorFactory(IServiceProvider serviceProvider) : ITypeGeneratorFactory<OpenApiUnknownResponse>
    {
        private static readonly ObjectFactory<UnknownResponseTypeGenerator> Factory =
            ActivatorUtilities.CreateFactory<UnknownResponseTypeGenerator>(
            [
                typeof(ILocatedOpenApiElement<OpenApiUnknownResponse>)
            ]);

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiUnknownResponse> element, ITypeGenerator? parent) =>
            Factory(serviceProvider, [element]);
    }
}
