using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Internal;
using Yardarm.Enrichment.Schema;
using Yardarm.Generation;
using Yardarm.Generation.Internal;
using Yardarm.Generation.MediaType;
using Yardarm.Generation.Operation;
using Yardarm.Generation.RequestBody;
using Yardarm.Generation.Response;
using Yardarm.Generation.Schema;
using Yardarm.Generation.Tag;
using Yardarm.Names;
using Yardarm.Packaging;
using Yardarm.Packaging.Internal;

namespace Yardarm
{
    public static class YardarmCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddYardarm(this IServiceCollection services, YardarmGenerationSettings settings, OpenApiDocument document)
        {
            services.AddDefaultEnrichers();

            // Generators
            services
                .AddTransient<IReferenceGenerator, NuGetReferenceGenerator>()
                .AddTransient<ISyntaxTreeGenerator, AssemblyInfoGenerator>()
                .AddTransient<ISyntaxTreeGenerator, SchemaGenerator>()
                .AddTransient<ISyntaxTreeGenerator, RequestBodyGenerator>()
                .AddTransient<ISyntaxTreeGenerator, ResponseGenerator>()
                .AddTransient<ISyntaxTreeGenerator, OperationGenerator>()
                .AddTransient<ISyntaxTreeGenerator, TagGenerator>()
                .AddTransient<IDependencyGenerator, StandardDependencyGenerator>();

            services.TryAddSingleton<ITypeGeneratorRegistry, TypeGeneratorRegistry>();
            services.TryAdd(new ServiceDescriptor(typeof(ITypeGeneratorRegistry<>), typeof(TypeGeneratorRegistry<>), ServiceLifetime.Singleton));
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiSchema>, DefaultSchemaGeneratorFactory>();

            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiRequestBody>, RequestBodyTypeGeneratorFactory>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiResponse>, ResponseTypeGeneratorFactory>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiOperation>, DefaultOperationGeneratorFactory>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiTag>, TagTypeGeneratorFactory>();
            services.TryAddSingleton<IMediaTypeSelector, JsonMediaTypeSelector>();

            services.TryAddSingleton<IPackageSpecGenerator, DefaultPackageSpecGenerator>();
            services.TryAddSingleton(serviceProvider => serviceProvider.GetRequiredService<IPackageSpecGenerator>().Generate());

            // Names
            services.TryAddSingleton<CamelCaseNameFormatter>();
            services.TryAddSingleton<PascalCaseNameFormatter>();
            services.TryAddSingleton<INameFormatterSelector, DefaultNameFormatterSelector>();
            services.TryAddSingleton<ITypeNameProvider, DefaultTypeNameProvider>();
            services.TryAddSingleton<INamespaceProvider, DefaultNamespaceProvider>();
            services.TryAddSingleton<IHttpResponseCodeNameProvider, HttpResponseCodeNameProvider>();

            // Other
            services
                .AddLogging()
                .AddSingleton<GenerationContext>()
                .AddSingleton(settings)
                .AddSingleton(document)
                .AddTransient<NuGetPacker>();

            return services;
        }


    }
}
