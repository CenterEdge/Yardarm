﻿using Microsoft.Extensions.DependencyInjection;

namespace Yardarm.Enrichment.Compilation
{
    public static class CompilationEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultCompilationEnrichers(this IServiceCollection services) =>
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

        public static IServiceCollection AddAssemblyInfoEnricher<T>(this IServiceCollection services)
            where T : class, IAssemblyInfoEnricher =>
            services.AddTransient<IAssemblyInfoEnricher, T>();

        public static IServiceCollection AddCompilationEnricher<T>(this IServiceCollection services)
            where T : class, ICompilationEnricher =>
            services.AddTransient<ICompilationEnricher, T>();

        public static IServiceCollection AddResourceFileEnricher<T>(this IServiceCollection services)
            where T : class, IResourceFileEnricher =>
            services.AddTransient<IResourceFileEnricher, T>();
    }
}
