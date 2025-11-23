using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment.Schema.Internal;

namespace Yardarm.Enrichment.Schema;

public static class SchemaEnricherServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDefaultSchemaEnrichers() =>
            services
                .AddOpenApiSyntaxNodeEnricher<RequiredPropertyEnricher>()
                .AddOpenApiSyntaxNodeEnricher<DocumentationPropertyEnricher>()
                .AddOpenApiSyntaxNodeEnricher<BaseTypeEnricher>()
                .AddOpenApiSyntaxNodeEnricher<AdditionalPropertiesEnricher>()
                .AddSchemaEnrichersCore();

        public IServiceCollection AddSchemaEnrichersCore()
        {
            services.TryAddSingleton<ISchemaBaseTypeRegistry, SchemaBaseTypeRegistry>();

            return services;
        }
    }
}
