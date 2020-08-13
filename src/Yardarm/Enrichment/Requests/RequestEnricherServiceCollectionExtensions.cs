using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment.Requests.Internal;

namespace Yardarm.Enrichment.Requests
{
    public static class RequestEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultRequestEnrichers(this IServiceCollection services) =>
            services
                .AddOpenApiSyntaxNodeEnricher<RequestClassMethodDocumentationEnricher>()
                .AddOpenApiSyntaxNodeEnricher<RequestInterfaceMethodDocumentationEnricher>()
                .AddOpenApiSyntaxNodeEnricher<RequestParameterDocumentationEnricher>();
    }
}
