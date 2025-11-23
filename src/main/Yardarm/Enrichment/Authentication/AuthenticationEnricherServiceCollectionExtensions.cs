using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment.Compilation;

namespace Yardarm.Enrichment.Authentication;

public static class AuthenticationEnricherServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDefaultAuthenticationEnrichers() =>
            services
                .AddOpenApiSyntaxNodeEnricher<SecuritySchemeDocumentationEnricher>()
                .AddOpenApiSyntaxNodeEnricher<SecuritySchemeRequestEnricher>()
                .AddResourceFileEnricher<AuthenticatorsSchemeEnricher>();
    }
}
