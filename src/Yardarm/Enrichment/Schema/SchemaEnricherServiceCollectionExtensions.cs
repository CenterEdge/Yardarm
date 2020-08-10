using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yardarm.Enrichment.Schema.Internal;

namespace Yardarm.Enrichment.Schema
{
    public static class SchemaEnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultSchemaEnrichers(this IServiceCollection services)
        {
            services
                .AddSchemaPropertyEnricher<RequiredPropertyEnricher>()
                .AddSchemaPropertyEnricher<DocumentationPropertyEnricher>()
                .AddSchemaClassEnricher<BaseTypeEnricher>();

            return services.AddSchemaEnrichersCore();
        }

        public static IServiceCollection AddSchemaEnrichersCore(this IServiceCollection services)
        {
            services.TryAddSingleton<ISchemaEnrichers, SchemaEnrichers>();

            return services;
        }

        public static IServiceCollection AddSchemaEnumEnricher<T>(this IServiceCollection services)
            where T : class, ISchemaEnumEnricher =>
            services.AddTransient<ISchemaEnumEnricher, T>();

        public static IServiceCollection AddSchemaEnumMemberEnricher<T>(this IServiceCollection services)
            where T : class, ISchemaEnumMemberEnricher =>
            services.AddTransient<ISchemaEnumMemberEnricher, T>();

        public static IServiceCollection AddSchemaClassEnricher<T>(this IServiceCollection services)
            where T : class, ISchemaClassEnricher =>
            services.AddTransient<ISchemaClassEnricher, T>();

        public static IServiceCollection AddSchemaInterfaceEnricher<T>(this IServiceCollection services)
            where T : class, ISchemaInterfaceEnricher =>
            services.AddTransient<ISchemaInterfaceEnricher, T>();

        public static IServiceCollection AddSchemaPropertyEnricher<T>(this IServiceCollection services)
            where T : class, ISchemaPropertyEnricher =>
            services.AddTransient<ISchemaPropertyEnricher, T>();
    }
}
