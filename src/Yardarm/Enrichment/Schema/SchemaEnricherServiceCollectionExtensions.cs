using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment.Schema.Internal;

namespace Yardarm.Enrichment.Schema
{
    public static class SchemaEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultSchemaEnrichers(this IServiceCollection services) =>
            services
                .AddOpenApiSyntaxNodeEnricher<RequiredPropertyEnricher>()
                .AddOpenApiSyntaxNodeEnricher<DocumentationPropertyEnricher>()
                .AddOpenApiSyntaxNodeEnricher<BaseTypeEnricher>();
    }
}
