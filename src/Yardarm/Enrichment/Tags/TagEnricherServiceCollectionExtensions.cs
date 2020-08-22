using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment.Tags.Internal;

namespace Yardarm.Enrichment.Tags
{
    public static class TagEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultTagEnrichers(this IServiceCollection services) =>
            services
                .AddOpenApiSyntaxNodeEnricher<DeprecatedOperationEnricher>();
    }
}
