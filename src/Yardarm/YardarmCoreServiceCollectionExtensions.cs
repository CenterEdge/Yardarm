using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Internal;
using Yardarm.Generation;
using Yardarm.Generation.Internal;
using Yardarm.Generation.Schema;
using Yardarm.Names;

namespace Yardarm
{
    public static class YardarmCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddYardarm(this IServiceCollection services, YardarmGenerationSettings settings, GenerationContext generationContext)
        {
            // Enrichers
            services
                .AddAssemblyInfoEnricher<TargetRuntimeAssemblyInfoEnricher>()
                .AddPropertyEnricher<RequiredPropertyEnricher>();

            // Generators
            services
                .AddTransient<IReferenceGenerator, NetStandardReferenceGenerator>()
                .AddTransient<ISyntaxTreeGenerator, AssemblyInfoGenerator>()
                .AddTransient<ISyntaxTreeGenerator, SchemaGenerator>();

            services.TryAddSingleton<ISchemaGeneratorFactory, DefaultSchemaGeneratorFactory>();
            services.TryAddSingleton<ObjectSchemaGenerator>();

            // Names
            services.TryAddSingleton<CamelCaseNameFormatter>();
            services.TryAddSingleton<PascalCaseNameFormatter>();
            services.TryAddSingleton<INameFormatterSelector, DefaultNameFormatterSelector>();
            services.TryAddSingleton<ITypeNameGenerator, DefaultTypeNameGenerator>();
            services.TryAddSingleton<INamespaceProvider, DefaultNamespaceProvider>();

            // Other
            services.AddSingleton(generationContext);
            services.AddSingleton(settings);
            services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<GenerationContext>().Document);

            return services;
        }

        public static IServiceCollection AddAssemblyInfoEnricher<T>(this IServiceCollection services)
            where T : class, IAssemblyInfoEnricher =>
            services.AddTransient<IAssemblyInfoEnricher, T>();

        public static IServiceCollection AddPropertyEnricher<T>(this IServiceCollection services)
            where T : class, IPropertyEnricher =>
            services.AddTransient<IPropertyEnricher, T>();
    }
}
