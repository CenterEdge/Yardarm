using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class ResponseSetTypeGeneratorFactory(IServiceProvider serviceProvider) : ITypeGeneratorFactory<OpenApiResponses>
    {
        private static readonly ObjectFactory<ResponseSetTypeGenerator> Factory =
            ActivatorUtilities.CreateFactory<ResponseSetTypeGenerator>(
            [
                typeof(ILocatedOpenApiElement<OpenApiResponses>)
            ]);

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiResponses> element, ITypeGenerator? parent) =>
            Factory(serviceProvider, [element]);
    }
}
