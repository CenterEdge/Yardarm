using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
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
        public static IServiceCollection AddYardarm(this IServiceCollection services, YardarmGenerationSettings settings, OpenApiDocument document)
        {
            // Enrichers
            services
                .AddAssemblyInfoEnricher<TargetRuntimeAssemblyInfoEnricher>()
                .AddPropertyEnricher<RequiredPropertyEnricher>()
                .AddPackageSpecEnricher<DependencyPackageSpecEnricher>()
                .AddSchemaClassEnricher<BaseTypeEnricher>();

            services.TryAddSingleton<IEnrichers, Enrichers>();

            // Generators
            services
                .AddTransient<IReferenceGenerator, NuGetReferenceGenerator>()
                .AddTransient<ISyntaxTreeGenerator, AssemblyInfoGenerator>()
                .AddTransient<ISyntaxTreeGenerator, SchemaGenerator>()
                .AddTransient<ISyntaxTreeGenerator, RequestBodyGenerator>()
                .AddTransient<ISyntaxTreeGenerator, ResponseGenerator>()
                .AddTransient<IDependencyGenerator, StandardDependencyGenerator>();

            services.TryAddSingleton<ISchemaGeneratorFactory, DefaultSchemaGeneratorFactory>();
            services.TryAddSingleton<ISchemaGeneratorRegistry, SchemaGeneratorRegistry>();

            services.TryAddSingleton<IRequestBodySchemaGenerator, RequestBodySchemaGenerator>();
            services.TryAddSingleton<IResponseSchemaGenerator, ResponseSchemaGenerator>();
            services.TryAddSingleton<IMediaTypeSelector, JsonMediaTypeSelector>();

            services.TryAddSingleton<IPackageSpecGenerator, DefaultPackageSpecGenerator>();
            services.TryAddSingleton(serviceProvider => serviceProvider.GetRequiredService<IPackageSpecGenerator>().Generate());

            // Names
            services.TryAddSingleton<CamelCaseNameFormatter>();
            services.TryAddSingleton<PascalCaseNameFormatter>();
            services.TryAddSingleton<INameFormatterSelector, DefaultNameFormatterSelector>();
            services.TryAddSingleton<ITypeNameGenerator, DefaultTypeNameGenerator>();
            services.TryAddSingleton<INamespaceProvider, DefaultNamespaceProvider>();
            services.TryAddSingleton<IHttpResponseCodeNameProvider, HttpResponseCodeNameProvider>();

            // Other
            services
                .AddLogging()
                .AddSingleton<GenerationContext>()
                .AddSingleton(settings)
                .AddSingleton(document);

            return services;
        }

        public static IServiceCollection AddAssemblyInfoEnricher<T>(this IServiceCollection services)
            where T : class, IAssemblyInfoEnricher =>
            services.AddTransient<IAssemblyInfoEnricher, T>();

        public static IServiceCollection AddEnumEnricher<T>(this IServiceCollection services)
            where T : class, IEnumEnricher =>
            services.AddTransient<IEnumEnricher, T>();

        public static IServiceCollection AddEnumMemberEnricher<T>(this IServiceCollection services)
            where T : class, IEnumMemberEnricher =>
            services.AddTransient<IEnumMemberEnricher, T>();

        public static IServiceCollection AddSchemaClassEnricher<T>(this IServiceCollection services)
            where T : class, ISchemaClassEnricher =>
            services.AddTransient<ISchemaClassEnricher, T>();

        public static IServiceCollection AddSchemaInterfaceEnricher<T>(this IServiceCollection services)
            where T : class, ISchemaInterfaceEnricher =>
            services.AddTransient<ISchemaInterfaceEnricher, T>();

        public static IServiceCollection AddPackageSpecEnricher<T>(this IServiceCollection services)
            where T : class, IPackageSpecEnricher =>
            services.AddTransient<IPackageSpecEnricher, T>();

        public static IServiceCollection AddPropertyEnricher<T>(this IServiceCollection services)
            where T : class, IPropertyEnricher =>
            services.AddTransient<IPropertyEnricher, T>();
    }
}
