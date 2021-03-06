using Microsoft.Extensions.DependencyInjection;

namespace Yardarm.Enrichment.Packaging
{
    public static class PackagingEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultPackagingEnrichers(this IServiceCollection services) =>
            services
                .AddPackageSpecEnricher<DependencyPackageSpecEnricher>();

        public static IServiceCollection AddNuGetPackageEnricher<T>(this IServiceCollection services)
            where T : class, INuGetPackageEnricher =>
            services.AddTransient<INuGetPackageEnricher, T>();

        public static IServiceCollection AddPackageSpecEnricher<T>(this IServiceCollection services)
            where T : class, IPackageSpecEnricher =>
            services.AddTransient<IPackageSpecEnricher, T>();
    }
}
