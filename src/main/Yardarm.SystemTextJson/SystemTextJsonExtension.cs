using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Compilation;
using Yardarm.Generation;
using Yardarm.Packaging;
using Yardarm.Serialization;
using Yardarm.SystemTextJson.Internal;

namespace Yardarm.SystemTextJson
{
    public class SystemTextJsonExtension : YardarmExtension
    {
        public override bool IsOutputTrimmable(GenerationContext context) => true;

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
                .AddOpenApiSyntaxNodeEnricher<JsonDateOnlyPropertyEnricher>()
                .AddSingleton<IDependencyGenerator, JsonDependencyGenerator>()
                .AddSingleton<ISyntaxTreeGenerator, ClientGenerator>()
                .AddSingleton<ISyntaxTreeGenerator, DiscriminatorConverterGenerator>()
                .AddSingleton<ISyntaxTreeGenerator, JsonSerializerContextGenerator>()
                .AddSingleton<ICompilationEnricher, JsonSerializableEnricher>()
                .TryAddTypeGeneratorFactory<OpenApiSchema, SystemTextJsonGeneratorCategory, DiscriminatorConverterTypeGeneratorFactory>();

            services
                .TryAddSingleton<IJsonSerializationNamespace, JsonSerializationNamespace>();

            services.AddSerializerDescriptor(serviceProvider => new SerializerDescriptor(
                ImmutableHashSet.Create(
                    new SerializerMediaType("application/json", 1.0),
                    new SerializerMediaType("text/json", 0.9),
                    // This is very low priority because we can't really use it for requests, since we don't know what the "*" should be.
                    // However, we don't want to generate HttpContent-based types unnecessarily. Swashbuckle-generated OpenAPI specs like
                    // to include this in the list of supported request bodies along with the other content types.
                    new SerializerMediaType("application/*+json", 0)),
                "Json",
                serviceProvider.GetRequiredService<IJsonSerializationNamespace>().JsonTypeSerializer
            ));

            services.AddSerializerDescriptor(serviceProvider => new SerializerDescriptor(
                ImmutableHashSet.Create(new SerializerMediaType("application/json-patch+json", 1.0)),
                "JsonPatch",
                serviceProvider.GetRequiredService<IJsonSerializationNamespace>().JsonTypeSerializer
            ));

            return services;
        }
    }
}
