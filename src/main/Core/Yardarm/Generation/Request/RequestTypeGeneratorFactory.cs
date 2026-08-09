using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class RequestTypeGeneratorFactory(IServiceProvider serviceProvider) : ITypeGeneratorFactory<OpenApiOperation>
    {
        private static readonly ObjectFactory<RequestTypeGenerator> Factory =
            ActivatorUtilities.CreateFactory<RequestTypeGenerator>(
            [
                typeof(ILocatedOpenApiElement<OpenApiOperation>)
            ]);

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiOperation> element, ITypeGenerator? parent) =>
            Factory(serviceProvider, [element]);
    }
}
