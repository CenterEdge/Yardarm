using System;
using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment.Authentication;
using Yardarm.Enrichment.Compilation;
using Yardarm.Enrichment.Packaging;
using Yardarm.Enrichment.Registration;
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

        public static IServiceCollection AddRegistrationEnricher<T>(this IServiceCollection services, string registrationType)
            where T : class, IRegistrationEnricher
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(registrationType);

            return services.AddKeyedTransient<IRegistrationEnricher, T>(registrationType);
        }

        public static IServiceCollection AddCreateDefaultRegistryEnricher<T>(this IServiceCollection services)
            where T : class, ICreateDefaultRegistryEnricher =>
            services.AddRegistrationEnricher<T>(DefaultTypeSerializersEnricher.RegistrationEnricherKey);

        public static IServiceCollection AddDefaultLiteralConverterEnricher<T>(this IServiceCollection services)
            where T : class, IDefaultLiteralConverterEnricher =>
            services.AddRegistrationEnricher<T>(DefaultLiteralConvertersEnricher.RegistrationEnricherKey);

        public static IServiceCollection AddOpenApiSyntaxNodeEnricher<T>(this IServiceCollection services)
            where T : class, IOpenApiSyntaxNodeEnricher =>
            services.AddTransient<IOpenApiSyntaxNodeEnricher, T>();
    }
}
