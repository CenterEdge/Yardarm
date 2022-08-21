using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment.Compilation;

namespace Yardarm.Enrichment.Authentication
{
    public static class AuthenticationEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultAuthenticationEnrichers(this IServiceCollection services) =>
            services
                .AddOpenApiSyntaxNodeEnricher<SecuritySchemeDocumentationEnricher>()
                .AddOpenApiSyntaxNodeEnricher<SecuritySchemeRequestEnricher>()
                .AddResourceFileEnricher<AuthenticatorsSchemeEnricher>();
    }
}
