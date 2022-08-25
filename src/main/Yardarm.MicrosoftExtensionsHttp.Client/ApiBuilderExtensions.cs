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

            var clientBuilder = builder.Services.AddHttpClient<TClient, TImplementation>((serviceProvider, httpClient) =>
            {
                var apiFactory = serviceProvider.GetRequiredService<ApiFactory>();

                apiFactory.ApplyHttpClientActions(httpClient);
            });

            configureClient?.Invoke(clientBuilder);

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
    }
}
