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

namespace Yardarm.Enrichment;

public static class EnricherServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDefaultEnrichers() =>
            services
                .AddDefaultCompilationEnrichers()
                .AddDefaultPackagingEnrichers()
                .AddDefaultAuthenticationEnrichers()
                .AddDefaultSchemaEnrichers()
                .AddDefaultRequestEnrichers()
                .AddDefaultResponseEnrichers()
                .AddDefaultTagEnrichers();

        public IServiceCollection AddRegistrationEnricher<T>(string registrationType)
            where T : class, IRegistrationEnricher
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(registrationType);

            return services.AddKeyedTransient<IRegistrationEnricher, T>(registrationType);
        }

        public IServiceCollection AddCreateDefaultRegistryEnricher<T>()
            where T : class, ICreateDefaultRegistryEnricher =>
            services.AddRegistrationEnricher<T>(DefaultTypeSerializersEnricher.RegistrationEnricherKey);

        public IServiceCollection AddDefaultLiteralConverterEnricher<T>()
            where T : class, IDefaultLiteralConverterEnricher =>
            services.AddRegistrationEnricher<T>(DefaultLiteralConvertersEnricher.RegistrationEnricherKey);

        public IServiceCollection AddOpenApiSyntaxNodeEnricher<T>()
            where T : class, IOpenApiSyntaxNodeEnricher =>
            services.AddTransient<IOpenApiSyntaxNodeEnricher, T>();
    }
}
