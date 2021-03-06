using Microsoft.Extensions.DependencyInjection;

namespace Yardarm.Enrichment.Tags
{
    public static class TagEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultTagEnrichers(this IServiceCollection services) =>
            services
                .AddOpenApiSyntaxNodeEnricher<DeprecatedOperationEnricher>();
    }
}
