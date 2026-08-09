using Microsoft.Extensions.DependencyInjection;

namespace Yardarm.Enrichment.Tags;

public static class TagEnricherServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDefaultTagEnrichers() =>
            services
                .AddOpenApiSyntaxNodeEnricher<DeprecatedOperationEnricher>();
    }
}
