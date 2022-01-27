using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Response
{
    public class ResponseTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiResponse>
    {
        private readonly IServiceProvider _serviceProvider;

        public ResponseTypeGeneratorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiResponse> element, ITypeGenerator? parent) =>
            ActivatorUtilities.CreateInstance<ResponseTypeGenerator>(_serviceProvider, element);
    }
}
