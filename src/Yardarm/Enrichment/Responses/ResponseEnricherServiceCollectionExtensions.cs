using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment.Responses.Internal;

namespace Yardarm.Enrichment.Responses
{
    public static class ResponseEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultResponseEnrichers(this IServiceCollection services) =>
            services
                .AddOpenApiSyntaxNodeEnricher<BaseTypeEnricher>()
                .AddOpenApiSyntaxNodeEnricher<HeaderDocumentationEnricher>();
    }
}
