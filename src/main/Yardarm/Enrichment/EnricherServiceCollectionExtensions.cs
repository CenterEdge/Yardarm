using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment.Authentication;
using Yardarm.Enrichment.Compilation;
using Yardarm.Enrichment.Packaging;
using Yardarm.Enrichment.Requests;
using Yardarm.Enrichment.Responses;
using Yardarm.Enrichment.Schema;
using Yardarm.Enrichment.Tags;

namespace Yardarm.Enrichment
{
    public static class EnricherServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultEnrichers(this IServiceCollection services) =>
            services
                .AddDefaultCompilationEnrichers()
                .AddDefaultPackagingEnrichers()
                .AddDefaultAuthenticationEnrichers()
                .AddDefaultSchemaEnrichers()
                .AddDefaultRequestEnrichers()
                .AddDefaultResponseEnrichers()
                .AddDefaultTagEnrichers();

        public static IServiceCollection AddCreateDefaultRegistryEnricher<T>(this IServiceCollection services)
            where T : class, ICreateDefaultRegistryEnricher =>
            services.AddTransient<ICreateDefaultRegistryEnricher, T>();

        public static IServiceCollection AddDefaultLiteralConverterEnricher<T>(this IServiceCollection services)
            where T : class, IDefaultLiteralConverterEnricher =>
            services.AddTransient<IDefaultLiteralConverterEnricher, T>();

        public static IServiceCollection AddOpenApiSyntaxNodeEnricher<T>(this IServiceCollection services)
            where T : class, IOpenApiSyntaxNodeEnricher =>
            services.AddTransient<IOpenApiSyntaxNodeEnricher, T>();
    }
}
