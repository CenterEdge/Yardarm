using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Interfaces;
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
using Yardarm.Internal;
using Yardarm.Names;
using Yardarm.Names.Internal;
using Yardarm.Packaging;
using Yardarm.Packaging.Internal;
using Yardarm.Serialization;
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
                .AddTransient<ISyntaxTreeGenerator, HeaderGenerator>()
                .AddTransient<ISyntaxTreeGenerator, MediaTypeGenerator>()
                .AddTransient<ISyntaxTreeGenerator, ParameterGenerator>()
                .AddTransient<ISyntaxTreeGenerator, SchemaGenerator>()
                .AddTransient<ISyntaxTreeGenerator, SecuritySchemeGenerator>()
                .AddTransient<ISyntaxTreeGenerator, ResponseGenerator>()
                .AddTransient<ISyntaxTreeGenerator, ResponseSetGenerator>()
                .AddTransient<ISyntaxTreeGenerator, RequestGenerator>()
                .AddTransient<ISyntaxTreeGenerator, TagGenerator>()
                .AddTransient<IDependencyGenerator, StandardDependencyGenerator>();

            services.TryAddSingleton<ITypeGeneratorRegistry, TypeGeneratorRegistry>();
            services.TryAdd(new ServiceDescriptor(typeof(ITypeGeneratorRegistry<,>), typeof(TypeGeneratorRegistry<,>), ServiceLifetime.Singleton));
            services.TryAdd(new ServiceDescriptor(typeof(ITypeGeneratorRegistry<>),
                typeof(PrimaryGeneratorCategory.TypeGeneratorRegistryWrapper<>), ServiceLifetime.Singleton));

            services.TryAdd(new ServiceDescriptor(typeof(ITypeGeneratorFactory<,>),
                typeof(NoopTypeGeneratorFactory<,>), ServiceLifetime.Singleton));
            services.TryAddTypeGeneratorFactory<OpenApiHeader, HeaderTypeGeneratorFactory>();
            services.TryAddTypeGeneratorFactory<OpenApiMediaType, MediaTypeGeneratorFactory>();
            services.TryAddTypeGeneratorFactory<OpenApiSchema, DefaultSchemaGeneratorFactory>();
            services.TryAddTypeGeneratorFactory<OpenApiSecurityScheme, SecuritySchemeTypeGeneratorFactory>();
            services.TryAddTypeGeneratorFactory<OpenApiResponse, ResponseTypeGeneratorFactory>();
            services.TryAddTypeGeneratorFactory<OpenApiResponses, ResponseSetTypeGeneratorFactory>();
            services.TryAddTypeGeneratorFactory<OpenApiOperation, RequestTypeGeneratorFactory>();
            services.TryAddTypeGeneratorFactory<OpenApiParameter, ParameterTypeGeneratorFactory>();
            services.TryAddTypeGeneratorFactory<OpenApiTag, TagTypeGeneratorFactory>();
            services.TryAddTypeGeneratorFactory<OpenApiUnknownResponse, UnknownResponseTypeGeneratorFactory>();

            services.AddSingleton<IRequestMemberGenerator, AddHeadersMethodGenerator>();
            services.AddSingleton<IRequestMemberGenerator, BuildContentMethodGenerator>();
            services.AddSingleton<IRequestMemberGenerator, BuildRequestMethodGenerator>();
            services.AddSingleton<IRequestMemberGenerator, BuildUriMethodGenerator>();
            services.TryAddSingleton<IGetBodyMethodGenerator, GetBodyMethodGenerator>();
            services.TryAddSingleton<IOperationMethodGenerator, OperationMethodGenerator>();
            services.TryAddSingleton<IMediaTypeSelector, PriorityMediaTypeSelector>();

            // Need to be able to specifically inject this one as well
            services.TryAddSingleton(serviceProvider =>
                serviceProvider.GetRequiredService<IEnumerable<IRequestMemberGenerator>>()
                    .OfType<IBuildContentMethodGenerator>().First());

            services.TryAddSingleton<IPackageSpecGenerator, DefaultPackageSpecGenerator>();
            services.TryAddSingleton(serviceProvider => serviceProvider.GetRequiredService<IPackageSpecGenerator>().Generate());

            // Names
            services.TryAddSingleton<CamelCaseNameFormatter>();
            services.TryAddSingleton<PascalCaseNameFormatter>();
            services.TryAddSingleton<INameFormatterSelector, NameFormatterSelector>();
            services.TryAddSingleton<INamespaceProvider, DefaultNamespaceProvider>();
            services.TryAddSingleton<INameConverterRegistry>(_ => NameConverterRegistry.CreateDefaultRegistry());
            services.TryAddSingleton<IHttpResponseCodeNameProvider, HttpResponseCodeNameProvider>();
            services.TryAddSingleton<IRootNamespace, RootNamespace>();
            services.TryAddSingleton<IAuthenticationNamespace, AuthenticationNamespace>();
            services.TryAddSingleton<IRequestsNamespace, RequestsNamespace>();
            services.TryAddSingleton<IResponsesNamespace, ResponsesNamespace>();
            services.TryAddSingleton<ISerializationNamespace, SerializationNamespace>();

            // Serialization
            services.TryAddSingleton<ISerializerSelector, DefaultSerializerSelector>();
            services.AddSerializerDescriptor(serviceProvider => new SerializerDescriptor(
                ImmutableHashSet.Create(new SerializerMediaType("multipart/form-data", 0.9)),
                "Multipart",
                serviceProvider.GetRequiredService<ISerializationNamespace>().MultipartFormDataSerializer));

            // Other
            services
                .AddLogging()
                .AddSingleton<GenerationContext>()
                .AddSingleton<YardarmAssemblyLoadContext>()
                .AddTransient<NuGetRestoreProcessor>()
                .AddSingleton(settings)
                .AddSingleton(document)
                .AddTransient<NuGetPacker>();

            services.TryAddSingleton<IOpenApiElementRegistry, OpenApiElementRegistry>();

            return services;
        }

        public static IServiceCollection AddSerializerDescriptor(this IServiceCollection services,
            SerializerDescriptor descriptor) =>
            services.AddSingleton(descriptor ?? throw new ArgumentNullException(nameof(descriptor)));

        public static IServiceCollection AddSerializerDescriptor(this IServiceCollection services,
            Func<IServiceProvider, SerializerDescriptor> descriptorFactory) =>
            services.AddSingleton(descriptorFactory ?? throw new ArgumentNullException(nameof(descriptorFactory)));

        public static void TryAddTypeGeneratorFactory<TElement, TGenerator>(this IServiceCollection services)
            where TElement : IOpenApiElement
            where TGenerator : class, ITypeGeneratorFactory<TElement, PrimaryGeneratorCategory> =>
            services.TryAddTypeGeneratorFactory<TElement, PrimaryGeneratorCategory, TGenerator>();

        public static void TryAddTypeGeneratorFactory<TElement, TGeneratorCategory, TGenerator>(this IServiceCollection services)
            where TElement : IOpenApiElement
            where TGenerator : class, ITypeGeneratorFactory<TElement, TGeneratorCategory> =>
            services.TryAddSingleton<ITypeGeneratorFactory<TElement, TGeneratorCategory>, TGenerator>();
    }
}
