using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
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
                .AddOpenApiSyntaxNodeEnricher<JsonDiscriminatorEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonNodeEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonAdditionalPropertiesEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonOptionalPropertyEnricher>()
                .AddSingleton<IDependencyGenerator, JsonDependencyGenerator>()
                .AddSingleton<ISyntaxTreeGenerator, ClientGenerator>()
                .AddSingleton<ISyntaxTreeGenerator, DiscriminatorConverterGenerator>()
                .TryAddTypeGeneratorFactory<OpenApiSchema, SystemTextJsonGeneratorCategory, DiscriminatorConverterTypeGeneratorFactory>();

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
