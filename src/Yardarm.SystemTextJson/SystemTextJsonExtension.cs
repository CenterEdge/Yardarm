using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.Packaging;
using Yardarm.Serialization;
using Yardarm.SystemTextJson.Internal;

namespace Yardarm.SystemTextJson
{
    public class SystemTextJsonExtension : YardarmExtension
    {
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services
                .AddCreateDefaultRegistryEnricher<JsonCreateDefaultRegistryEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonPropertyEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonEnumEnricher>()
                .AddSingleton<IDependencyGenerator, JsonDependencyGenerator>()
                .AddSingleton<ISyntaxTreeGenerator, ClientGenerator>();

            services
                .TryAddSingleton<IJsonSerializationNamespace, JsonSerializationNamespace>();

            services.AddSerializerDescriptor(serviceProvider => new SerializerDescriptor(
                ImmutableHashSet.Create(new SerializerMediaType("application/json", 1.0)),
                "Json",
                serviceProvider.GetRequiredService<IJsonSerializationNamespace>().JsonTypeSerializer
            ));

            return services;
        }
    }
}
