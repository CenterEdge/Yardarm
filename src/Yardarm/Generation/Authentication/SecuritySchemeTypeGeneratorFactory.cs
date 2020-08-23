using System;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Authentication
{
    public class SecuritySchemeTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiSecurityScheme>
    {
        private readonly GenerationContext _context;
        private readonly IAuthenticationNamespace _authenticationNamespace;

        public SecuritySchemeTypeGeneratorFactory(GenerationContext context, IAuthenticationNamespace authenticationNamespace)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _authenticationNamespace = authenticationNamespace ?? throw new ArgumentNullException(nameof(authenticationNamespace));
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiSecurityScheme> element) =>
            element.Element.Type switch
            {
                SecuritySchemeType.Http => element.Element.Scheme switch
                {
                    "bearer" => new BearerSecuritySchemeTypeGenerator(element, _context, _authenticationNamespace),
                    _ => throw new InvalidOperationException($"Unsupported HTTP authentication scheme {element.Element.Scheme}.")
                },
                _ => throw new InvalidOperationException($"Unsupported authentication type {element.Element.Type}.")
            };
    }
}
