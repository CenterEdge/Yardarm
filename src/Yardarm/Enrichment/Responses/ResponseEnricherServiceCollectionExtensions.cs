using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment.Responses.Internal;

namespace Yardarm.Enrichment.Responses
{
    public static class ResponseEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultResponseEnrichers(this IServiceCollection services)
        {
            services
                .AddResponseHeaderPropertyEnricher<ResponseHeaderDocumentationEnricher>();

            return services.AddResponseEnrichersCore();
        }

        public static IServiceCollection AddResponseEnrichersCore(this IServiceCollection services)
        {
            services.TryAddSingleton<IResponseEnrichers, ResponseEnrichers>();

            return services;
        }

        public static IServiceCollection AddResponseHeaderPropertyEnricher<T>(this IServiceCollection services)
            where T : class, IResponseHeaderPropertyEnricher =>
            services.AddTransient<IResponseHeaderPropertyEnricher, T>();
    }
}
