using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RootNamespace.Api;
using RootNamespace.Internal;
using Yardarm.Client.Internal;

namespace RootNamespace
{
    /// <summary>
    /// Extensions for <see cref="IApiBuilder"/>.
    /// </summary>
    public static class ApiBuilderExtensions
    {
        /// <summary>
        /// Add all APIs with their default concrete implementation.
        /// </summary>
        /// <param name="builder">The <see cref="IApiBuilder"/>.</param>
        /// <param name="configureClient">Action to configure the <see cref="IHttpClientBuilder"/> called for each API. The first parameter is the type of the API interface.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// This method may not be combined with calls to AddApi to add specific APIs. If APIs are added individually,
        /// they must all be added individually.
        /// </para>
        /// </remarks>
        public static IApiBuilder AddAllApis(this IApiBuilder builder, Action<Type, IHttpClientBuilder>? configureClient = null)
        {
            builder.AddAllApisInternal(configureClient, false);

            return builder;
        }

        /// <summary>
        /// Add all unregistered APIs with their default concrete implementation.
        /// </summary>
        /// <param name="builder">The <see cref="IApiBuilder"/>.</param>
        /// <param name="configureClient">Action to configure the <see cref="IHttpClientBuilder"/> called for each API. The first parameter is the type of the API interface.</param>
        /// <remarks>
        /// <para>
        /// This method is called last on the <see cref="IApiBuilder"/> and only registers APIs which are not already
        /// registered individually.
        /// </para>
        /// </remarks>
        public static void AddAllOtherApis(this IApiBuilder builder, Action<Type, IHttpClientBuilder>? configureClient = null)
        {
            builder.AddAllApisInternal(configureClient, true);
        }

        /// <summary>
        /// Add an API with its concrete implementation to the <see cref="IApiBuilder"/> and the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TClient">The API interface.</typeparam>
        /// <typeparam name="TImplementation">The concrete API implementation.</typeparam>
        /// <param name="builder">The <see cref="IApiBuilder"/>.</param>
        /// <param name="configureClient">An action to configure the <see cref="HttpClient"/> used by the API.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
        /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
        /// <typeparamref name="TClient"/> as the service type.
        /// </para>
        /// </remarks>
        public static IApiBuilder AddApi<TClient,
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            TImplementation>(this IApiBuilder builder,
            Action<IHttpClientBuilder>? configureClient = null)
            where TClient : class, IApi
            where TImplementation : class, TClient
        {
            ThrowHelper.ThrowIfNull(builder, nameof(builder));

            if (!ApiClientMappingRegistry.TryReserve(builder.Services, typeof(TClient)))
            {
                ThrowRegistrationError(typeof(TClient));
            }

            var clientBuilder = builder.Services.AddHttpClient<TClient, TImplementation>((serviceProvider, httpClient) =>
            {
                var apiFactory = serviceProvider.GetRequiredService<ApiFactory>();

                apiFactory.ApplyHttpClientActions(httpClient);
            });

            configureClient?.Invoke(clientBuilder);

            return builder;
        }

        // Internal implementation used by AddAllApis and AddAllOtherApis
        private static void AddAllApisInternal(this IApiBuilder builder,
            Action<Type, IHttpClientBuilder>? configureClient,
            bool skipIfAlreadyRegistered)
        {
            // This area will be dynamically enriched with calls to builder.AddApi<X, Y>(configureClient, skipIfAlreadyRegistered);
        }

        // Internal implementation used by AddAllApisInternal
        private static void AddApi<TClient,
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            TImplementation>(this IApiBuilder builder,
            Action<Type, IHttpClientBuilder>? configureClient,
            bool skipIfAlreadyRegistered)
            where TClient : class, IApi
            where TImplementation : class, TClient
        {
            ThrowHelper.ThrowIfNull(builder, nameof(builder));

            if (!ApiClientMappingRegistry.TryReserve(builder.Services, typeof(TClient)))
            {
                if (!skipIfAlreadyRegistered)
                {
                    ThrowRegistrationError(typeof(TClient));
                }
                return;
            }

            var clientBuilder = builder.Services.AddHttpClient<TClient, TImplementation>(
                (serviceProvider, httpClient) =>
                {
                    var apiFactory = serviceProvider.GetRequiredService<ApiFactory>();

                    apiFactory.ApplyHttpClientActions(httpClient);
                });

            configureClient?.Invoke(typeof(TClient), clientBuilder);
        }

        /// <summary>
        /// Apply common configuration to the <see cref="HttpClient"/> for all APIs.
        /// </summary>
        /// <param name="builder">The <see cref="IApiBuilder"/>.</param>
        /// <param name="configureClient">An action to configure the <see cref="HttpClient"/>.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="configureClient"/> will be executed before any other configuration applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public static IApiBuilder ConfigureHttpClient(this IApiBuilder builder, Action<HttpClient> configureClient)
        {
            ThrowHelper.ThrowIfNull(builder, nameof(builder));
            ThrowHelper.ThrowIfNull(configureClient, nameof(configureClient));

            builder.Services.Configure<ApiFactoryOptions>(options => options.HttpClientActions.Add(configureClient));

            return builder;
        }

        /// <summary>
        /// Apply common configuration to the <see cref="HttpClient"/> for all APIs.
        /// </summary>
        /// <param name="builder">The <see cref="IApiBuilder"/>.</param>
        /// <param name="configureClient">An action to configure the <see cref="HttpClient"/>.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="configureClient"/> will be executed before any other configuration applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public static IApiBuilder ConfigureHttpClient(this IApiBuilder builder, Action<IServiceProvider, HttpClient> configureClient)
        {
            ThrowHelper.ThrowIfNull(builder, nameof(builder));
            ThrowHelper.ThrowIfNull(configureClient, nameof(configureClient));

            builder.Services.AddTransient<IConfigureOptions<ApiFactoryOptions>>(services =>
            {
                return new ConfigureOptions<ApiFactoryOptions>(options =>
                {
                    options.HttpClientActions.Add(client => configureClient(services, client));
                });
            });

            return builder;
        }

        /// <summary>
        /// Apply a default base <see cref="Uri"/> for all APIs.
        /// </summary>
        /// <param name="builder">The <see cref="IApiBuilder"/>.</param>
        /// <param name="uri">The base URI. The URI should generally end with a trailing "/".</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The base URI will be applied to the <see cref="HttpClient"/> before any other configuration applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public static IApiBuilder ConfigureBaseAddress(this IApiBuilder builder, Uri uri)
        {
            ThrowHelper.ThrowIfNull(builder, nameof(builder));
            ThrowHelper.ThrowIfNull(uri, nameof(uri));

            return builder.ConfigureHttpClient(client => client.BaseAddress = uri);
        }

        [DoesNotReturn]
        private static void ThrowRegistrationError(Type clientType)
        {
            throw new InvalidOperationException(
                $"The API {clientType} has already been registered. It may only be registered once.");
        }
    }
}
