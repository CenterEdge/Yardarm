using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Authentication
{
    public class SecuritySchemeTypeGeneratorFactory(IServiceProvider serviceProvider) : ITypeGeneratorFactory<OpenApiSecurityScheme>
    {
        private static ObjectFactory<ApiKeyHeaderSecuritySchemeTypeGenerator>? _apiKeyHeaderFactory;
        private static ObjectFactory<ApiKeyHeaderSecuritySchemeTypeGenerator> ApiKeyHeaderFactory => _apiKeyHeaderFactory ??=
            ActivatorUtilities.CreateFactory<ApiKeyHeaderSecuritySchemeTypeGenerator>(
            [
                typeof(ILocatedOpenApiElement<OpenApiSecurityScheme>)
            ]);

        private static ObjectFactory<ApiKeyQuerySecuritySchemeTypeGenerator>? _apiKeyQueryFactory;
        private static ObjectFactory<ApiKeyQuerySecuritySchemeTypeGenerator> ApiKeyQueryFactory => _apiKeyQueryFactory ??=
            ActivatorUtilities.CreateFactory<ApiKeyQuerySecuritySchemeTypeGenerator>(
            [
                typeof(ILocatedOpenApiElement<OpenApiSecurityScheme>)
            ]);

        private static ObjectFactory<BasicSecuritySchemeTypeGenerator>? _basicFactory;
        private static ObjectFactory<BasicSecuritySchemeTypeGenerator> BasicFactory => _basicFactory ??=
            ActivatorUtilities.CreateFactory<BasicSecuritySchemeTypeGenerator>(
            [
                typeof(ILocatedOpenApiElement<OpenApiSecurityScheme>)
            ]);

        private static ObjectFactory<BearerSecuritySchemeTypeGenerator>? _bearerFactory;
        private static ObjectFactory<BearerSecuritySchemeTypeGenerator> BearerFactory => _bearerFactory ??=
            ActivatorUtilities.CreateFactory<BearerSecuritySchemeTypeGenerator>(
            [
                typeof(ILocatedOpenApiElement<OpenApiSecurityScheme>)
            ]);

        private static ObjectFactory<NoopSecuritySchemeTypeGenerator>? _noopFactory;
        private static ObjectFactory<NoopSecuritySchemeTypeGenerator> NoopFactory => _noopFactory ??=
            ActivatorUtilities.CreateFactory<NoopSecuritySchemeTypeGenerator>(
            [
                typeof(ILocatedOpenApiElement<OpenApiSecurityScheme>)
            ]);

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiSecurityScheme> element, ITypeGenerator? parent)
        {
            return element.Element.Type switch
            {
                SecuritySchemeType.Http => element.Element.Scheme switch
                {
                    "bearer" => BearerFactory(serviceProvider, [element]),
                    "basic" => BasicFactory(serviceProvider, [element]),
                    _ => NoopFactory(serviceProvider, [element])
                },
                SecuritySchemeType.ApiKey => element.Element.In switch
                {
                    ParameterLocation.Header => ApiKeyHeaderFactory(serviceProvider, [element]),
                    ParameterLocation.Query => ApiKeyQueryFactory(serviceProvider, [element]),
                    _ => NoopFactory(serviceProvider, [element])
                },
                _ => NoopFactory(serviceProvider, [element])
            };
        }
    }
}
