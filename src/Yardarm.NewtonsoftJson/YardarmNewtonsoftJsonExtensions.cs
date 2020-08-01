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
                    .AddSingleton<IPropertyEnricher, JsonPropertyEnricher>()
                    .AddSingleton<IDependencyGenerator, JsonDependencyGenerator>();

                return services;
            });
    }
}
