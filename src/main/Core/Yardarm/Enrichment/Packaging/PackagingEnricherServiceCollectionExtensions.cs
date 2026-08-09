using Microsoft.Extensions.DependencyInjection;

namespace Yardarm.Enrichment.Packaging;

public static class PackagingEnricherServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDefaultPackagingEnrichers() =>
            services
                .AddPackageSpecEnricher<DependencyPackageSpecEnricher>();

        public IServiceCollection AddNuGetPackageEnricher<T>()
            where T : class, INuGetPackageEnricher =>
            services.AddTransient<INuGetPackageEnricher, T>();

        public IServiceCollection AddPackageSpecEnricher<T>()
            where T : class, IPackageSpecEnricher =>
            services.AddTransient<IPackageSpecEnricher, T>();
    }
}
