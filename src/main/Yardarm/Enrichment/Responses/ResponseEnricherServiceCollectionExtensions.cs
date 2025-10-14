using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment.Responses.Internal;

namespace Yardarm.Enrichment.Responses;

public static class ResponseEnricherServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDefaultResponseEnrichers() =>
            services
                .AddOpenApiSyntaxNodeEnricher<BaseTypeEnricher>()
                .AddOpenApiSyntaxNodeEnricher<HeaderDocumentationEnricher>()
                .AddOpenApiSyntaxNodeEnricher<HeaderParsingEnricher>()
                .AddOpenApiSyntaxNodeEnricher<ResponseTypeCastExtensionEnricher>()
                .AddOpenApiSyntaxNodeEnricher<RequiredHeaderEnricher>()
                .AddResponseEnrichersCore();

        public IServiceCollection AddResponseEnrichersCore()
        {
            services.TryAddSingleton<IResponseBaseTypeRegistry, ResponseBaseTypeRegistry>();

            return services;
        }
    }
}
