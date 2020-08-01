using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment;
using Yardarm.Generation;

namespace Yardarm.NewtonsoftJson
{
    public static class YardarmNewtonsoftJsonExtensions
    {
        public static YardarmGenerationSettings AddNewtonsoftJson(this YardarmGenerationSettings settings) =>
            settings.AddExtension(services =>
            {
                services
                    .AddPropertyEnricher<JsonPropertyEnricher>()
                    .AddEnumEnricher<JsonEnumEnricher>()
                    .AddSingleton<IDependencyGenerator, JsonDependencyGenerator>();

                return services;
            });
    }
}
