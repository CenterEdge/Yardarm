using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment.Internal;
using Yardarm.Enrichment.Responses;
using Yardarm.Enrichment.Schema;

namespace Yardarm.Enrichment
{
    public static class EnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultEnrichers(this IServiceCollection services)
        {
            services
                .AddAssemblyInfoEnricher<TargetRuntimeAssemblyInfoEnricher>()
                .AddAssemblyInfoEnricher<VersionAssemblyInfoEnricher>()
                .AddOperationClassMethodEnricher<OperationClassMethodDocumentationEnricher>()
                .AddOperationInterfaceMethodEnricher<OperationInterfaceMethodDocumentationEnricher>()
                .AddPackageSpecEnricher<DependencyPackageSpecEnricher>()
                .AddDefaultSchemaEnrichers()
                .AddDefaultResponseEnrichers();

            return services.AddEnrichersCore();
        }

        public static IServiceCollection AddEnrichersCore(this IServiceCollection services)
        {
            services.TryAddSingleton<IEnrichers, Enrichers>();

            return services;
        }

        public static IServiceCollection AddAssemblyInfoEnricher<T>(this IServiceCollection services)
            where T : class, IAssemblyInfoEnricher =>
            services.AddTransient<IAssemblyInfoEnricher, T>();

        public static IServiceCollection AddNuGetPackageEnricher<T>(this IServiceCollection services)
            where T : class, INuGetPackageEnricher =>
            services.AddTransient<INuGetPackageEnricher, T>();

        public static IServiceCollection AddOperationClassMethodEnricher<T>(this IServiceCollection services)
            where T : class, IOperationClassMethodEnricher =>
            services.AddTransient<IOperationClassMethodEnricher, T>();

        public static IServiceCollection AddOperationInterfaceMethodEnricher<T>(this IServiceCollection services)
            where T : class, IOperationInterfaceMethodEnricher =>
            services.AddTransient<IOperationInterfaceMethodEnricher, T>();

        public static IServiceCollection AddPackageSpecEnricher<T>(this IServiceCollection services)
            where T : class, IPackageSpecEnricher =>
            services.AddTransient<IPackageSpecEnricher, T>();
    }
}
