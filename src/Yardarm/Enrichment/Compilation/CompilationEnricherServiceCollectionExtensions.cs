using Microsoft.Extensions.DependencyInjection;

namespace Yardarm.Enrichment.Compilation
{
    public static class CompilationEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultCompilationEnrichers(this IServiceCollection services) =>
            services
                .AddAssemblyInfoEnricher<TargetRuntimeAssemblyInfoEnricher>()
                .AddAssemblyInfoEnricher<VersionAssemblyInfoEnricher>()
                .AddCompilationEnricher<ReferenceCompilationEnricher>()
                .AddCompilationEnricher<ResourceFileCompilationEnricher>()
                .AddCompilationEnricher<SyntaxTreeCompilationEnricher>()
                .AddCompilationEnricher<OpenApiCompilationEnricher>()
                .AddCompilationEnricher<DefaultTypeSerializersEnricher>();

        public static IServiceCollection AddAssemblyInfoEnricher<T>(this IServiceCollection services)
            where T : class, IAssemblyInfoEnricher =>
            services.AddTransient<IAssemblyInfoEnricher, T>();

        public static IServiceCollection AddCreateDefaultRegistryEnricher<T>(this IServiceCollection services)
            where T : class, ICreateDefaultRegistryEnricher =>
            services.AddTransient<ICreateDefaultRegistryEnricher, T>();

        public static IServiceCollection AddCompilationEnricher<T>(this IServiceCollection services)
            where T : class, ICompilationEnricher =>
            services.AddTransient<ICompilationEnricher, T>();

        public static IServiceCollection AddResourceFileEnricher<T>(this IServiceCollection services)
            where T : class, IResourceFileEnricher =>
            services.AddTransient<IResourceFileEnricher, T>();
    }
}
