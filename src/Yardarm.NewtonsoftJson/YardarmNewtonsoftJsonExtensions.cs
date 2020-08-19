using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.NewtonsoftJson.Internal;
using Yardarm.Packaging;

namespace Yardarm.NewtonsoftJson
{
    public static class YardarmNewtonsoftJsonExtensions
    {
        public static YardarmGenerationSettings AddNewtonsoftJson(this YardarmGenerationSettings settings) =>
            settings.AddExtension(services =>
            {
                services
                    .AddCreateDefaultRegistryEnricher<JsonCreateDefaultRegistryEnricher>()
                    .AddOpenApiSyntaxNodeEnricher<JsonPropertyEnricher>()
                    .AddOpenApiSyntaxNodeEnricher<JsonEnumEnricher>()
                    .AddOpenApiSyntaxNodeEnricher<JsonDiscriminatorEnricher>()
                    .AddSingleton<IDependencyGenerator, JsonDependencyGenerator>()
                    .AddSingleton<ISyntaxTreeGenerator, ClientGenerator>();

                services
                    .TryAddSingleton<IJsonSerializationNamespace, JsonSerializationNamespace>();

                return services;
            });
    }
}
