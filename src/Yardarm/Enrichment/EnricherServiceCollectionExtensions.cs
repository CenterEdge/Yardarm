using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment.Internal;
using Yardarm.Enrichment.Requests;
using Yardarm.Enrichment.Responses;
using Yardarm.Enrichment.Schema;

namespace Yardarm.Enrichment
{
    public static class EnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultEnrichers(this IServiceCollection services) =>
            services
                .AddAssemblyInfoEnricher<TargetRuntimeAssemblyInfoEnricher>()
                .AddAssemblyInfoEnricher<VersionAssemblyInfoEnricher>()
                .AddCompilationEnricher<ReferenceCompilationEnricher>()
                .AddCompilationEnricher<SyntaxTreeCompilationEnricher>()
                .AddCompilationEnricher<OpenApiCompilationEnricher>()
                .AddPackageSpecEnricher<DependencyPackageSpecEnricher>()
                .AddDefaultSchemaEnrichers()
                .AddDefaultRequestEnrichers()
                .AddDefaultResponseEnrichers();

        public static IServiceCollection AddAssemblyInfoEnricher<T>(this IServiceCollection services)
            where T : class, IAssemblyInfoEnricher =>
            services.AddTransient<IAssemblyInfoEnricher, T>();

        public static IServiceCollection AddCompilationEnricher<T>(this IServiceCollection services)
            where T : class, ICompilationEnricher =>
            services.AddTransient<ICompilationEnricher, T>();

        public static IServiceCollection AddNuGetPackageEnricher<T>(this IServiceCollection services)
            where T : class, INuGetPackageEnricher =>
            services.AddTransient<INuGetPackageEnricher, T>();

        public static IServiceCollection AddOpenApiSyntaxNodeEnricher<T>(this IServiceCollection services)
            where T : class, IOpenApiSyntaxNodeEnricher =>
            services.AddTransient<IOpenApiSyntaxNodeEnricher, T>();

        public static IServiceCollection AddPackageSpecEnricher<T>(this IServiceCollection services)
            where T : class, IPackageSpecEnricher =>
            services.AddTransient<IPackageSpecEnricher, T>();
    }
}
