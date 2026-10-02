using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Compilation;
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
                .AddCreateDefaultRegistryEnricher<JsonLinesCreateDefaultRegistryEnricher>()
                .AddResourceFileEnricher<JsonSerializerSettingsEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonAdditionalPropertiesEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonPropertyEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonEnumEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonDiscriminatorEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonDateOnlyPropertyEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonDiscriminatedUnionEnricher>()
                .AddOpenApiSyntaxNodeEnricher<JsonExtensibleEnumEnricher>()
                .AddSingleton<IDependencyGenerator, JsonDependencyGenerator>()
                .AddSingleton<ISyntaxTreeGenerator, ClientGenerator>();

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

            services.AddSerializerDescriptor(serviceProvider => new SerializerDescriptor(
                ImmutableHashSet.Create(
                    // Lower priority than application/json and text/json so that JSON is preferred when both are offered
                    new SerializerMediaType("application/jsonl", 0.8),
                    new SerializerMediaType("application/x-ndjson", 0.7)),
                "JsonLines",
                serviceProvider.GetRequiredService<IJsonSerializationNamespace>().JsonLinesTypeSerializer
            ));

            return services;
        }
    }
}
