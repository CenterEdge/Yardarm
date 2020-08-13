using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment;
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
                    .AddOpenApiSyntaxNodeEnricher<JsonEnumEnricher>()
                    .AddOpenApiSyntaxNodeEnricher<JsonDiscriminatorEnricher>()
                    .AddSingleton<IDependencyGenerator, JsonDependencyGenerator>()
                    .AddSingleton<ISyntaxTreeGenerator, JsonSyntaxTreeGenerator>();

                return services;
            });
    }
}
