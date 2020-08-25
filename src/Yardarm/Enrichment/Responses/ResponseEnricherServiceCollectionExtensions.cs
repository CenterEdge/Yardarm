using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment.Responses.Internal;

namespace Yardarm.Enrichment.Responses
{
    public static class ResponseEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultResponseEnrichers(this IServiceCollection services) =>
            services
                .AddOpenApiSyntaxNodeEnricher<BaseTypeEnricher>()
                .AddOpenApiSyntaxNodeEnricher<HeaderDocumentationEnricher>()
                .AddOpenApiSyntaxNodeEnricher<HeaderParsingEnricher>()
                .AddOpenApiSyntaxNodeEnricher<ResponseTypeCastExtensionEnricher>()
                .AddResponseEnrichersCore();

        public static IServiceCollection AddResponseEnrichersCore(this IServiceCollection services)
        {
            services.TryAddSingleton<IResponseBaseTypeRegistry, ResponseBaseTypeRegistry>();

            return services;
        }
    }
}
