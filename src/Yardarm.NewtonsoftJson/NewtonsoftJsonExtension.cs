using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.NewtonsoftJson.Internal;
using Yardarm.Packaging;
using Yardarm.Serialization;

namespace Yardarm.NewtonsoftJson
{
    public class NewtonsoftJsonExtension : YardarmExtension
    {
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services
                .AddCreateDefaultRegistryEnricher<JsonCreateDefaultRegistryEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonAdditionalPropertiesEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonPropertyEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonEnumEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonDiscriminatorEnricher>()
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
