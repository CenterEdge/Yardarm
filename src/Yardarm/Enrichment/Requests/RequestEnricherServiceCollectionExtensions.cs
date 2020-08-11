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
                .AddOperationClassMethodEnricher<OperationClassMethodDocumentationEnricher>()
                .AddOperationInterfaceMethodEnricher<OperationInterfaceMethodDocumentationEnricher>();

            return services.AddRequestEnrichersCore();
        }

        public static IServiceCollection AddRequestEnrichersCore(this IServiceCollection services)
        {
            services.TryAddSingleton<IRequestEnrichers, RequestEnrichers>();

            return services;
        }

        public static IServiceCollection AddOperationClassMethodEnricher<T>(this IServiceCollection services)
            where T : class, IOperationClassMethodEnricher =>
            services.AddTransient<IOperationClassMethodEnricher, T>();

        public static IServiceCollection AddOperationInterfaceMethodEnricher<T>(this IServiceCollection services)
            where T : class, IOperationInterfaceMethodEnricher =>
            services.AddTransient<IOperationInterfaceMethodEnricher, T>();
    }
}
