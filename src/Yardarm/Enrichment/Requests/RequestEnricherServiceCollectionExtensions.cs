using Microsoft.Extensions.DependencyInjection;

namespace Yardarm.Enrichment.Requests
{
    public static class RequestEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultRequestEnrichers(this IServiceCollection services) =>
            services
                .AddOpenApiSyntaxNodeEnricher<RequestClassMethodDocumentationEnricher>()
                .AddOpenApiSyntaxNodeEnricher<RequestInterfaceMethodDocumentationEnricher>()
                .AddOpenApiSyntaxNodeEnricher<RequestParameterDocumentationEnricher>()
                .AddOpenApiSyntaxNodeEnricher<RequiredBodyRequestEnricher>()
                .AddOpenApiSyntaxNodeEnricher<RequiredParameterEnricher>();
    }
}
