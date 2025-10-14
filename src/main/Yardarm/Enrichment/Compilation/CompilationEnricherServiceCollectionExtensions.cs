using Microsoft.Extensions.DependencyInjection;

namespace Yardarm.Enrichment.Compilation;

public static class CompilationEnricherServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDefaultCompilationEnrichers() =>
            services
                .AddAssemblyInfoEnricher<TargetRuntimeAssemblyInfoEnricher>()
                .AddAssemblyInfoEnricher<VersionAssemblyInfoEnricher>()
                .AddAssemblyInfoEnricher<IsTrimmableAssemblyInfoEnricher>()
                .AddCompilationEnricher<ReferenceCompilationEnricher>()
                .AddCompilationEnricher<ResourceFileCompilationEnricher>()
                .AddCompilationEnricher<SyntaxTreeCompilationEnricher>()
                .AddCompilationEnricher<OpenApiCompilationEnricher>()
                .AddCompilationEnricher<FormatCompilationEnricher>()
                .AddResourceFileEnricher<DefaultTypeSerializersEnricher>()
                .AddResourceFileEnricher<DefaultLiteralConvertersEnricher>();

        public IServiceCollection AddAssemblyInfoEnricher<T>()
            where T : class, IAssemblyInfoEnricher =>
            services.AddTransient<IAssemblyInfoEnricher, T>();

        public IServiceCollection AddCompilationEnricher<T>()
            where T : class, ICompilationEnricher =>
            services.AddTransient<ICompilationEnricher, T>();

        public IServiceCollection AddResourceFileEnricher<T>()
            where T : class, IResourceFileEnricher =>
            services.AddTransient<IResourceFileEnricher, T>();
    }
}
