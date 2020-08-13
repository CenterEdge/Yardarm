using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Schema;
using Yardarm.Generation;
using Yardarm.Packaging;

namespace Yardarm.NewtonsoftJson
{
    public static class YardarmNewtonsoftJsonExtensions
    {
        public static YardarmGenerationSettings AddNewtonsoftJson(this YardarmGenerationSettings settings) =>
            settings.AddExtension(services =>
            {
                services
                    .AddOpenApiSyntaxNodeEnricher<JsonPropertyEnricher>()
                    .AddSchemaEnumEnricher<JsonEnumEnricher>()
                    .AddSchemaInterfaceEnricher<JsonDiscriminatorEnricher>()
                    .AddSingleton<IDependencyGenerator, JsonDependencyGenerator>()
                    .AddSingleton<ISyntaxTreeGenerator, JsonSyntaxTreeGenerator>();

                return services;
            });
    }
}
