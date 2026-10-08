using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Compilation;
using Yardarm.Generation;
using Yardarm.Packaging;
using Yardarm.Serialization;
using Yardarm.SystemTextJson.Internal;

namespace Yardarm.SystemTextJson;

public class SystemTextJsonExtension(YardarmGenerationSettings settings) : YardarmExtension
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
            .AddOpenApiSyntaxNodeEnricher<JsonDiscriminatedUnionEnricher>()
            .AddOpenApiSyntaxNodeEnricher<JsonExtensibleEnumEnricher>()
            .AddSingleton<IDependencyGenerator, JsonDependencyGenerator>()
            .AddSingleton<ISyntaxTreeGenerator, ClientGenerator>()
            .AddSingleton<ISyntaxTreeGenerator, DiscriminatorConverterGenerator>()
            .AddSingleton<ISyntaxTreeGenerator, JsonSerializerContextGenerator>()
            .AddSingleton<ICompilationEnricher, JsonSerializableEnricher>()
            .AddTypeGeneratorFactory<IOpenApiSchema, DiscriminatorConverterTypeGeneratorFactory>(DiscriminatorConverterTypeGenerator.GeneratorCategory);

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
            serviceProvider.GetRequiredService<IJsonSerializationNamespace>().JsonTypeSerializer,
            supportsStreaming: true
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
            serviceProvider.GetRequiredService<IJsonSerializationNamespace>().JsonLinesTypeSerializer,
            supportsStreaming: true
        ));

        services.Configure<JsonOptions>(options => options.ApplySettings(settings));

        return services;
    }
}
