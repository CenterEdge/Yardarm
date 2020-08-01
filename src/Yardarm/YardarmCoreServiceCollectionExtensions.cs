using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Internal;
using Yardarm.Generation;
using Yardarm.Generation.Api;
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
                .AddPropertyEnricher<RequiredPropertyEnricher>()
                .AddPackageSpecEnricher<DependencyPackageSpecEnricher>();

            // Generators
            services
                .AddTransient<IReferenceGenerator, NuGetReferenceGenerator>()
                .AddTransient<ISyntaxTreeGenerator, AssemblyInfoGenerator>()
                .AddTransient<ISyntaxTreeGenerator, SchemaGenerator>()
                .AddTransient<ISyntaxTreeGenerator, RequestBodyGenerator>()
                .AddTransient<IDependencyGenerator, StandardDependencyGenerator>();

            services.TryAddSingleton<ISchemaGeneratorFactory, DefaultSchemaGeneratorFactory>();
            services.TryAddSingleton<ObjectSchemaGenerator>();
            services.TryAddSingleton<ArraySchemaGenerator>();
            services.TryAddSingleton<NumberSchemaGenerator>();
            services.TryAddSingleton<StringSchemaGenerator>();
            services.TryAddSingleton<EnumSchemaGenerator>();

            services.TryAddSingleton<IRequestBodySchemaGenerator, RequestBodySchemaGenerator>();
            services.TryAddSingleton<IMediaTypeSelector, JsonMediaTypeSelector>();

            services.TryAddSingleton<IPackageSpecGenerator, DefaultPackageSpecGenerator>();
            services.TryAddSingleton(serviceProvider => serviceProvider.GetRequiredService<IPackageSpecGenerator>().Generate());

            // Names
            services.TryAddSingleton<CamelCaseNameFormatter>();
            services.TryAddSingleton<PascalCaseNameFormatter>();
            services.TryAddSingleton<INameFormatterSelector, DefaultNameFormatterSelector>();
            services.TryAddSingleton<ITypeNameGenerator, DefaultTypeNameGenerator>();
            services.TryAddSingleton<INamespaceProvider, DefaultNamespaceProvider>();

            // Other
            services
                .AddLogging()
                .AddSingleton(generationContext)
                .AddSingleton(settings)
                .AddSingleton(serviceProvider => serviceProvider.GetRequiredService<GenerationContext>().Document);

            return services;
        }

        public static IServiceCollection AddAssemblyInfoEnricher<T>(this IServiceCollection services)
            where T : class, IAssemblyInfoEnricher =>
            services.AddTransient<IAssemblyInfoEnricher, T>();

        public static IServiceCollection AddSchemaClassEnricher<T>(this IServiceCollection services)
            where T : class, ISchemaClassEnricher =>
            services.AddTransient<ISchemaClassEnricher, T>();

        public static IServiceCollection AddPackageSpecEnricher<T>(this IServiceCollection services)
            where T : class, IPackageSpecEnricher =>
            services.AddTransient<IPackageSpecEnricher, T>();

        public static IServiceCollection AddPropertyEnricher<T>(this IServiceCollection services)
            where T : class, IPropertyEnricher =>
            services.AddTransient<IPropertyEnricher, T>();
    }
}
