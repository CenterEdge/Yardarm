using System;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Authentication
{
    public class SecuritySchemeTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiSecurityScheme>
    {
        private readonly GenerationContext _context;
        private readonly IAuthenticationNamespace _authenticationNamespace;
        private readonly ILogger<NoopSecuritySchemeTypeGenerator> _logger;

        public SecuritySchemeTypeGeneratorFactory(GenerationContext context, IAuthenticationNamespace authenticationNamespace,
            ILogger<NoopSecuritySchemeTypeGenerator> logger)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(authenticationNamespace);
            ArgumentNullException.ThrowIfNull(logger);

            _context = context;
            _authenticationNamespace = authenticationNamespace;
            _logger = logger;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiSecurityScheme> element, ITypeGenerator? parent)
        {
            return element.Element.Type switch
            {
                SecuritySchemeType.Http => element.Element.Scheme switch
                {
                    "bearer" => new BearerSecuritySchemeTypeGenerator(element, _context, _authenticationNamespace),
                    "basic" => new BasicSecuritySchemeTypeGenerator(element, _context, _authenticationNamespace),
                    _ => new NoopSecuritySchemeTypeGenerator(element, _context, _authenticationNamespace, _logger)
                },
                SecuritySchemeType.ApiKey => element.Element.In switch
                {
                    ParameterLocation.Header => new ApiKeyHeaderSecuritySchemeTypeGenerator(element, _context, _authenticationNamespace),
                    _ => new NoopSecuritySchemeTypeGenerator(element, _context, _authenticationNamespace, _logger)
                },
                _ => new NoopSecuritySchemeTypeGenerator(element, _context, _authenticationNamespace, _logger)
            };
        }
    }
}
