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
        public static IServiceCollection AddYardarm(this IServiceCollection services)
        {
            // Enrichers
            services
                .AddTransient<IAssemblyInfoEnricher, TargetRuntimeAssemblyInfoEnricher>();

            // Generators
            services
                .AddTransient<IReferenceGenerator, NetStandardReferenceGenerator>()
                .AddTransient<ISyntaxTreeGenerator, AssemblyInfoGenerator>()
                .AddTransient<ISyntaxTreeGenerator, SchemaGenerator>();

            services.TryAddTransient<ISchemaGeneratorFactory, DefaultSchemaGeneratorFactory>();

            // Name formatters
            services.TryAddScoped<CamelCaseNameFormatter>();
            services.TryAddScoped<PascalCaseNameFormatter>();
            services.TryAddScoped<INameFormatterSelector, DefaultNameFormatterSelector>();

            // Other
            services.TryAddScoped<GenerationContextProvider>();
            services.TryAddTransient(serviceProvider => serviceProvider.GetRequiredService<GenerationContextProvider>().Context);
            services.TryAddTransient(serviceProvider => serviceProvider.GetRequiredService<GenerationContext>().Document);

            return services;
        }
    }
}
