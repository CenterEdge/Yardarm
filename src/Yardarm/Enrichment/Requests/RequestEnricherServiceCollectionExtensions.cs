using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment.Requests.Internal;

namespace Yardarm.Enrichment.Requests
{
    public static class RequestEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultRequestEnrichers(this IServiceCollection services)
        {
            services
                .AddRequestClassMethodEnricher<RequestClassMethodDocumentationEnricher>()
                .AddRequestInterfaceMethodEnricher<RequestInterfaceMethodDocumentationEnricher>()
                .AddRequestParameterPropertyEnricher<RequestParameterDocumentationEnricher>();

            return services.AddRequestEnrichersCore();
        }

        public static IServiceCollection AddRequestEnrichersCore(this IServiceCollection services)
        {
            services.TryAddSingleton<IRequestEnrichers, RequestEnrichers>();

            return services;
        }

        public static IServiceCollection AddRequestClassMethodEnricher<T>(this IServiceCollection services)
            where T : class, IRequestClassMethodEnricher =>
            services.AddTransient<IRequestClassMethodEnricher, T>();

        public static IServiceCollection AddRequestInterfaceMethodEnricher<T>(this IServiceCollection services)
            where T : class, IRequestInterfaceMethodEnricher =>
            services.AddTransient<IRequestInterfaceMethodEnricher, T>();

        public static IServiceCollection AddRequestParameterPropertyEnricher<T>(this IServiceCollection services)
            where T : class, IRequestParameterPropertyEnricher =>
            services.AddTransient<IRequestParameterPropertyEnricher, T>();
    }
}
