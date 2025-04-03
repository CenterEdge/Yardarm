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
        public static IServiceCollection AddYardarm(this IServiceCollection services, YardarmGenerationSettings settings, OpenApiDocument? document)
        {
            services.AddOptions();
            services.AddDefaultEnrichers();

            if (settings.ReferencedAssemblies is null || settings.ReferencedAssemblies.Count == 0)
            {
                services.AddTransient<IReferenceGenerator, NuGetReferenceGenerator>();
            }
            else
            {
                services.AddTransient<IReferenceGenerator, SuppliedReferenceGenerator>();
            }

            // Generators
            services
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
            services.TryAddTypeGeneratorRegistry(generatorCategory: null);

            services.AddTypeGeneratorFactory<OpenApiHeader, HeaderTypeGeneratorFactory>();
            services.AddTypeGeneratorFactory<OpenApiMediaType, MediaTypeGeneratorFactory>();
            services.AddTypeGeneratorFactory<OpenApiSchema, DefaultSchemaGeneratorFactory>();
            services.AddTypeGeneratorFactory<OpenApiSecurityScheme, SecuritySchemeTypeGeneratorFactory>();
            services.AddTypeGeneratorFactory<OpenApiResponse, ResponseTypeGeneratorFactory>();
            services.AddTypeGeneratorFactory<OpenApiResponses, ResponseSetTypeGeneratorFactory>();
            services.AddTypeGeneratorFactory<OpenApiOperation, RequestTypeGeneratorFactory>();
            services.AddTypeGeneratorFactory<OpenApiParameter, ParameterTypeGeneratorFactory>();
            services.AddTypeGeneratorFactory<OpenApiTag, TagTypeGeneratorFactory>();
            services.AddTypeGeneratorFactory<OpenApiTag, TagImplementationTypeGeneratorFactory>(TagImplementationTypeGenerator.GeneratorCategory);
            services.AddTypeGeneratorFactory<OpenApiUnknownResponse, UnknownResponseTypeGeneratorFactory>();

            services.AddSingleton<IRequestMemberGenerator, AddHeadersMethodGenerator>();
            services.AddSingleton<IRequestMemberGenerator, BuildContentMethodGenerator>();
            services.AddSingleton<IRequestMemberGenerator, BuildRequestMethodGenerator>();
            services.AddSingleton<IRequestMemberGenerator, BuildUriMethodGenerator>();
            services.AddSingleton<IRequestMemberGenerator, HttpMethodPropertyGenerator>();
            services.AddSingleton<IRequestMemberGenerator, SerializationDataPropertyGenerator>();
            services.AddSingleton<IResponseMethodGenerator, GetBodyMethodGenerator>();
            services.AddSingleton<IResponseMethodGenerator, BodyConstructorMethodGenerator>();
            services.AddSingleton<IResponseMethodGenerator, NoBodyConstructorMethodGenerator>();
            services.TryAddSingleton<IOperationMethodGenerator, OperationMethodGenerator>();
            services.TryAddSingleton<IOperationNameProvider, DefaultOperationNameProvider>();
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
            services.TryAddSingleton<IApiNamespace, ApiNamespace>();
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
                .AddTransient<NuGetRestoreProcessor>()
                .AddSingleton(settings)
                .AddTransient<NuGetPacker>();

            if (document is not null)
            {
                services
                    .AddSingleton<GenerationContext>()
                    // When requesting the YardarmContext simply return the GenerationContext
                    .AddTransient<YardarmContext>(serviceProvider => serviceProvider.GetRequiredService<GenerationContext>())
                    .AddSingleton(document);
            }
            else
            {
                // We don't have a document, so supply a basic YardarmContext only
                services.AddSingleton<YardarmContext>();
            }

            services.TryAddSingleton<IOpenApiElementRegistry, OpenApiElementRegistry>();

            services.ConfigureOptions<StringSchemaOptionsConfigurator>();

            return services;
        }

        public static IServiceCollection AddSerializerDescriptor(this IServiceCollection services,
            SerializerDescriptor descriptor)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(descriptor);

            return services.AddSingleton(descriptor);
        }

        public static IServiceCollection AddSerializerDescriptor(this IServiceCollection services,
            Func<IServiceProvider, SerializerDescriptor> descriptorFactory)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(descriptorFactory);

            return services.AddSingleton(descriptorFactory);
        }

        public static IServiceCollection AddTypeGeneratorFactory<TElement, TGenerator>(this IServiceCollection services)
            where TElement : IOpenApiElement
            where TGenerator : class, ITypeGeneratorFactory<TElement> =>
            services.AddTypeGeneratorFactory<TElement, TGenerator>(generatorCategory: null);

        public static IServiceCollection AddTypeGeneratorFactory<TElement, TGenerator>(this IServiceCollection services, string? generatorCategory)
            where TElement : IOpenApiElement
            where TGenerator : class, ITypeGeneratorFactory<TElement>
        {
            if (generatorCategory is not null)
            {
                // Ensure the keyed generator registry has been added. Default is added centrally for performance.
                services.TryAddTypeGeneratorRegistry(generatorCategory);
            }

            return services.AddKeyedSingleton<ITypeGeneratorFactory<TElement>, TGenerator>(generatorCategory);
        }

        [Obsolete("Use AddTypeGeneratorFactory instead, type generators now support multi registration.")]
        public static void TryAddTypeGeneratorFactory<TElement, TGenerator>(this IServiceCollection services)
            where TElement : IOpenApiElement
            where TGenerator : class, ITypeGeneratorFactory<TElement> =>
            services.TryAddSingleton<ITypeGeneratorFactory<TElement>, TGenerator>();

        private static void TryAddTypeGeneratorRegistry(this IServiceCollection services, string? generatorCategory)
        {
            services.TryAdd(ServiceDescriptor.KeyedSingleton(typeof(ITypeGeneratorRegistry<>), generatorCategory, typeof(TypeGeneratorRegistry<>)));
        }
    }
}
