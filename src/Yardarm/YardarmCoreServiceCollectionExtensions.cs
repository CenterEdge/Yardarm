using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.Generation.Authentication;
using Yardarm.Generation.Internal;
using Yardarm.Generation.MediaType;
using Yardarm.Generation.Operation;
using Yardarm.Generation.Request;
using Yardarm.Generation.Response;
using Yardarm.Generation.Schema;
using Yardarm.Generation.Tag;
using Yardarm.Names;
using Yardarm.Names.Internal;
using Yardarm.Packaging;
using Yardarm.Packaging.Internal;
using Yardarm.Spec;
using Yardarm.Spec.Internal;

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
                .AddTransient<ISyntaxTreeGenerator, ClientGenerator>()
                .AddTransient<ISyntaxTreeGenerator, SchemaGenerator>()
                .AddTransient<ISyntaxTreeGenerator, SecuritySchemeGenerator>()
                .AddTransient<ISyntaxTreeGenerator, RequestBodyGenerator>()
                .AddTransient<ISyntaxTreeGenerator, ResponseGenerator>()
                .AddTransient<ISyntaxTreeGenerator, ResponseSetGenerator>()
                .AddTransient<ISyntaxTreeGenerator, RequestGenerator>()
                .AddTransient<ISyntaxTreeGenerator, TagGenerator>()
                .AddTransient<IDependencyGenerator, StandardDependencyGenerator>();

            services.TryAddSingleton<ITypeGeneratorRegistry, TypeGeneratorRegistry>();
            services.TryAdd(new ServiceDescriptor(typeof(ITypeGeneratorRegistry<>), typeof(TypeGeneratorRegistry<>), ServiceLifetime.Singleton));

            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiHeader>, NoopTypeGeneratorFactory<OpenApiHeader>>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiMediaType>, NoopTypeGeneratorFactory<OpenApiMediaType>>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiSchema>, DefaultSchemaGeneratorFactory>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiSecurityScheme>, SecuritySchemeTypeGeneratorFactory>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiRequestBody>, RequestBodyTypeGeneratorFactory>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiResponse>, ResponseTypeGeneratorFactory>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiResponses>, ResponseSetTypeGeneratorFactory>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiOperation>, RequestTypeGeneratorFactory>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiParameter>, NoopTypeGeneratorFactory<OpenApiParameter>>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiPathItem>, NoopTypeGeneratorFactory<OpenApiPathItem>>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiTag>, TagTypeGeneratorFactory>();
            services.TryAddSingleton<ITypeGeneratorFactory<OpenApiUnknownResponse>, UnknownResponseTypeGeneratorFactory>();

            services.TryAddSingleton<IAddHeadersMethodGenerator, AddHeadersMethodGenerator>();
            services.TryAddSingleton<IBuildContentMethodGenerator, BuildContentMethodGenerator>();
            services.TryAddSingleton<IBuildRequestMethodGenerator, BuildRequestMethodGenerator>();
            services.TryAddSingleton<IBuildUriMethodGenerator, BuildUriMethodGenerator>();
            services.TryAddSingleton<IGetBodyMethodGenerator, GetBodyMethodGenerator>();
            services.TryAddSingleton<IOperationMethodGenerator, OperationMethodGenerator>();
            services.TryAddSingleton<IMediaTypeSelector, JsonMediaTypeSelector>();

            services.TryAddSingleton<IPackageSpecGenerator, DefaultPackageSpecGenerator>();
            services.TryAddSingleton(serviceProvider => serviceProvider.GetRequiredService<IPackageSpecGenerator>().Generate());

            // Names
            services.TryAddSingleton<CamelCaseNameFormatter>();
            services.TryAddSingleton<PascalCaseNameFormatter>();
            services.TryAddSingleton<INameFormatterSelector, DefaultNameFormatterSelector>();
            services.TryAddSingleton<INamespaceProvider, DefaultNamespaceProvider>();
            services.TryAddSingleton<IHttpResponseCodeNameProvider, HttpResponseCodeNameProvider>();
            services.TryAddSingleton<IRootNamespace, RootNamespace>();
            services.TryAddSingleton<IAuthenticationNamespace, AuthenticationNamespace>();
            services.TryAddSingleton<IRequestsNamespace, RequestsNamespace>();
            services.TryAddSingleton<IResponsesNamespace, ResponsesNamespace>();
            services.TryAddSingleton<ISerializationNamespace, SerializationNamespace>();

            // Other
            services
                .AddLogging()
                .AddSingleton<GenerationContext>()
                .AddSingleton(settings)
                .AddSingleton(document)
                .AddTransient<NuGetPacker>();

            services.TryAddSingleton<IOpenApiElementRegistry, OpenApiElementRegistry>();

            return services;
        }
    }
}
