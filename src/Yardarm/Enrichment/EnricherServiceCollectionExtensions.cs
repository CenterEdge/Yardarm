using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment.Internal;
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
                .AddOperationMethodEnricher<OperationMethodDocumentationEnricher>()
                .AddPackageSpecEnricher<DependencyPackageSpecEnricher>()
                .AddDefaultSchemaEnrichers();

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

        public static IServiceCollection AddOperationMethodEnricher<T>(this IServiceCollection services)
            where T : class, IOperationMethodEnricher =>
            services.AddTransient<IOperationMethodEnricher, T>();

        public static IServiceCollection AddPackageSpecEnricher<T>(this IServiceCollection services)
            where T : class, IPackageSpecEnricher =>
            services.AddTransient<IPackageSpecEnricher, T>();
    }
}
