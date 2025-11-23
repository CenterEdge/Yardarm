using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using RootNamespace.Api;
using RootNamespace.Internal;

namespace RootNamespace;

/// <summary>
/// Extensions for <see cref="IApiBuilder"/>.
/// </summary>
public static class ApiBuilderExtensions
{
    private static readonly TimeSpan s_minimumHandlerLifetime = TimeSpan.FromSeconds(1);

    /// <param name="builder">The <see cref="IApiBuilder"/>.</param>
    extension(IApiBuilder builder)
    {

        #region APIs

        /// <summary>
        /// Add all APIs with their default concrete implementation.
        /// </summary>
        /// <param name="configureClient">Action to configure the <see cref="IHttpClientBuilder"/> called for each API. The first parameter is the type of the API interface.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// This method may not be combined with calls to AddApi to add specific APIs. If APIs are added individually,
        /// they must all be added individually.
        /// </para>
        /// </remarks>
        public IApiBuilder AddAllApis(Action<Type, IHttpClientBuilder>? configureClient = null)
        {
            builder.AddAllApisInternal(configureClient, false);

            return builder;
        }

        /// <summary>
        /// Add all unregistered APIs with their default concrete implementation.
        /// </summary>
        /// <param name="configureClient">Action to configure the <see cref="IHttpClientBuilder"/> called for each API. The first parameter is the type of the API interface.</param>
        /// <remarks>
        /// <para>
        /// This method is called last on the <see cref="IApiBuilder"/> and only registers APIs which are not already
        /// registered individually.
        /// </para>
        /// </remarks>
        public void AddAllOtherApis(Action<Type, IHttpClientBuilder>? configureClient = null)
        {
            builder.AddAllApisInternal(configureClient, true);
        }

        /// <summary>
        /// Add an API with its concrete implementation to the <see cref="IApiBuilder"/> and the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TClient">The API interface.</typeparam>
        /// <typeparam name="TImplementation">The concrete API implementation.</typeparam>
        /// <param name="configureClient">An action to configure the <see cref="HttpClient"/> used by the API.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
        /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
        /// <typeparamref name="TClient"/> as the service type.
        /// </para>
        /// </remarks>
        public IApiBuilder AddApi<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
            Action<IHttpClientBuilder>? configureClient = null)
            where TClient : class, IApi
            where TImplementation : class, TClient
        {
            ArgumentNullException.ThrowIfNull(builder);

            if (!ApiClientMappingRegistry.TryReserve(builder.Services, typeof(TClient)))
            {
                ThrowRegistrationError(typeof(TClient));
            }

            var clientBuilder = builder.Services.AddHttpClient<TClient, TImplementation>();

            // Register common configuration before any custom per-API configuration
            AddCommonConfiguration(clientBuilder);

            configureClient?.Invoke(clientBuilder);

            return builder;
        }

        /// <summary>
        /// Add an API with its concrete implementation to the <see cref="IApiBuilder"/> and the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TClient">The API interface.</typeparam>
        /// <typeparam name="TImplementation">The concrete API implementation.</typeparam>
        /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
        /// <param name="configureClient">An action to configure the <see cref="HttpClient"/> used by the API.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
        /// can be retrieved from <see cref="IServiceProvider.GetService(Type)" /> (and related methods) by providing
        /// <typeparamref name="TClient"/> as the service type.
        /// </para>
        /// </remarks>
        public IApiBuilder AddApi<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
            string name,
            Action<IHttpClientBuilder>? configureClient = null)
            where TClient : class, IApi
            where TImplementation : class, TClient
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(name);

            if (!ApiClientMappingRegistry.TryReserve(builder.Services, typeof(TClient)))
            {
                ThrowRegistrationError(typeof(TClient));
            }

            var clientBuilder = builder.Services.AddHttpClient<TClient, TImplementation>(name);

            // Register common configuration before any custom per-API configuration
            AddCommonConfiguration(clientBuilder);

            configureClient?.Invoke(clientBuilder);

            return builder;
        }

        /// <summary>
        /// Add an API by name with its concrete implementation to the <see cref="IApiBuilder"/> and the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TClient">The concrete API implementation.</typeparam>
        /// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
        /// <param name="configureClient">An action to configure the <see cref="HttpClient"/> used by the API.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// <typeparamref name="TClient"/> instances constructed with the appropriate <see cref="HttpClient" />
        /// can be retrieved from <see cref="IHttpClientFactory" /> by providing the <paramref name="name"/>.
        /// </para>
        /// </remarks>
        public IApiBuilder AddApi<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TClient>(
            string name,
            Action<IHttpClientBuilder>? configureClient = null)
            where TClient : class
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(name);

            var clientBuilder = builder.Services.AddHttpClient<TClient>(name);

            // Register common configuration before any custom per-API configuration
            AddCommonConfiguration(clientBuilder);

            configureClient?.Invoke(clientBuilder);

            return builder;
        }

        // Internal implementation used by AddAllApis and AddAllOtherApis
        private void AddAllApisInternal(
            Action<Type, IHttpClientBuilder>? configureClient,
            bool skipIfAlreadyRegistered)
        {
            // This area will be dynamically enriched with calls to builder.AddApi<X, Y>(configureClient, name, skipIfAlreadyRegistered);
        }

        // Internal implementation used by AddAllApisInternal
        private void AddApi<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
            Action<Type, IHttpClientBuilder>? configureClient,
            string name,
            bool skipIfAlreadyRegistered)
            where TClient : class, IApi
            where TImplementation : class, TClient
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(name);

            if (!ApiClientMappingRegistry.TryReserve(builder.Services, typeof(TClient)))
            {
                if (!skipIfAlreadyRegistered)
                {
                    ThrowRegistrationError(typeof(TClient));
                }
                return;
            }

            var clientBuilder = builder.Services.AddHttpClient<TClient, TImplementation>(name);

            // Register common configuration before any customer per-API configuration
            AddCommonConfiguration(clientBuilder);

            configureClient?.Invoke(typeof(TClient), clientBuilder);
        }

        #endregion

        #region ConfigureAuthenticators

        /// <summary>
        /// Configure the default <see cref="Authentication.Authenticators"/>.
        /// </summary>
        /// <param name="configureAuthenticators">Delegate to configure the <see cref="Authentication.Authenticators"/>.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        public IApiBuilder ConfigureAuthenticators(
            Action<Authentication.Authenticators> configureAuthenticators)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configureAuthenticators);

            builder.Services.Configure<ApiFactoryOptions>(options =>
                options.AuthenticatorActions.Add(configureAuthenticators));

            return builder;
        }

        /// <summary>
        /// Configure the default <see cref="Authentication.Authenticators"/>.
        /// </summary>
        /// <param name="configureAuthenticators">Delegate to configure the <see cref="Authentication.Authenticators"/>.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        public IApiBuilder ConfigureAuthenticators(
            Action<IServiceProvider, Authentication.Authenticators> configureAuthenticators)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configureAuthenticators);

            builder.Services.AddTransient<IConfigureOptions<ApiFactoryOptions>>(services =>
            {
                return new ConfigureOptions<ApiFactoryOptions>(options =>
                {
                    options.AuthenticatorActions.Add(authenticators => configureAuthenticators(services, authenticators));
                });
            });

            return builder;
        }

        #endregion

        #region ConfigureHttpClient

        /// <summary>
        /// Apply common configuration to the <see cref="HttpClient"/> for all APIs.
        /// </summary>
        /// <param name="configureClient">An action to configure the <see cref="HttpClient"/>.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="configureClient"/> will be executed before any other configuration applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public IApiBuilder ConfigureHttpClient(Action<HttpClient> configureClient)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configureClient);

            builder.Services.Configure<ApiFactoryOptions>(options => options.HttpClientActions.Add(configureClient));

            return builder;
        }

        /// <summary>
        /// Apply common configuration to the <see cref="HttpClient"/> for all APIs.
        /// </summary>
        /// <param name="configureClient">An action to configure the <see cref="HttpClient"/>.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="configureClient"/> will be executed before any other configuration applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public IApiBuilder ConfigureHttpClient(Action<IServiceProvider, HttpClient> configureClient)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configureClient);

            builder.Services.AddTransient<IConfigureOptions<ApiFactoryOptions>>(services =>
            {
                return new ConfigureOptions<ApiFactoryOptions>(options =>
                {
                    options.HttpClientActions.Add(client => configureClient(services, client));
                });
            });

            return builder;
        }

        #endregion

        #region ConfigurePrimaryHttpMessageHandler

        /// <summary>
        /// Apply a default factory for the primary <see cref="HttpMessageHandler"/> for all APIs.
        /// </summary>
        /// <param name="configureHandler">A function to create the <see cref="HttpMessageHandler"/>.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="configureHandler"/> will be applied before any other configuration applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public IApiBuilder ConfigurePrimaryHttpMessageHandler(Func<HttpMessageHandler> configureHandler)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configureHandler);

            builder.Services.Configure<ApiFactoryOptions>(options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(b => b.PrimaryHandler = configureHandler());
            });

            return builder;
        }

        /// <summary>
        /// Apply a default factory for the primary <see cref="HttpMessageHandler"/> for all APIs.
        /// </summary>
        /// <param name="configureHandler">A function to create the <see cref="HttpMessageHandler"/>.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="configureHandler"/> will be applied before any other configuration applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public IApiBuilder ConfigurePrimaryHttpMessageHandler(Func<IServiceProvider, HttpMessageHandler> configureHandler)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configureHandler);

            builder.Services.Configure<ApiFactoryOptions>(options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(b => b.PrimaryHandler = configureHandler(b.Services));
            });

            return builder;
        }

        /// <summary>
        /// Apply a default factory for the primary <see cref="HttpMessageHandler"/> for all APIs.
        /// </summary>
        /// <typeparam name="THandler">The type of the <see cref="HttpMessageHandler"/> to request from the DI container.</typeparam>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <typeparamref name="THandler"/> will be applied before any other configuration applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public IApiBuilder ConfigurePrimaryHttpMessageHandler<THandler>()
            where THandler : HttpMessageHandler
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.Configure<ApiFactoryOptions>(options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(b => b.PrimaryHandler = b.Services.GetRequiredService<THandler>());
            });

            return builder;
        }

        #endregion

        #region AddHttpMessageHandler

        /// <summary>
        /// Adds a delegate that will be used to create an additional message handler for all APIs.
        /// </summary>
        /// <param name="configureHandler">A delegate used to create a <see cref="DelegatingHandler"/>.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <typeref name="DelegatingHandler"/> will be applied before any other handlers applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public IApiBuilder AddHttpMessageHandler(Func<DelegatingHandler> configureHandler)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configureHandler);

            builder.Services.Configure<ApiFactoryOptions>(options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(b => b.AdditionalHandlers.Add(configureHandler()));
            });

            return builder;
        }

        /// <summary>
        /// Adds a delegate that will be used to create an additional message handler for all APIs.
        /// </summary>
        /// <param name="configureHandler">A delegate used to create a <see cref="DelegatingHandler"/>.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <typeref name="DelegatingHandler"/> will be applied before any other handlers applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public IApiBuilder AddHttpMessageHandler(Func<IServiceProvider, DelegatingHandler> configureHandler)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configureHandler);

            builder.Services.Configure<ApiFactoryOptions>(options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(b => b.AdditionalHandlers.Add(configureHandler(b.Services)));
            });

            return builder;
        }

        /// <summary>
        /// Adds an additional message handler from the dependency injection container to all APIs.
        /// </summary>
        /// <typeparam name="THandler">The type of the <see cref="DelegatingHandler"/> to request from the DI container.</typeparam>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The <typeparamref name="THandler"/> will be applied before any other handlers applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public IApiBuilder AddHttpMessageHandler<THandler>()
            where THandler : DelegatingHandler
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.Configure<ApiFactoryOptions>(options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(b => b.AdditionalHandlers.Add(b.Services.GetRequiredService<THandler>()));
            });

            return builder;
        }

        #endregion

        #region ConfigureBaseAddress

        /// <summary>
        /// Apply a default base <see cref="Uri"/> for all APIs.
        /// </summary>
        /// <param name="uri">The base URI. The URI should generally end with a trailing "/".</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The base URI will be applied to the <see cref="HttpClient"/> before any other configuration applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public IApiBuilder ConfigureBaseAddress(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(uri);

            return builder.ConfigureHttpClient(client => client.BaseAddress = uri);
        }

        /// <summary>
        /// Apply a default base <see cref="Uri"/> for all APIs.
        /// </summary>
        /// <param name="configureUri">A delegate which retrieves the base URI. The URI should generally end with a trailing "/".</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The base URI will be applied to the <see cref="HttpClient"/> before any other configuration applied
        /// via the <see cref="IHttpClientBuilder"/> for a specific API.
        /// </para>
        /// </remarks>
        public IApiBuilder ConfigureBaseAddress(Func<IServiceProvider, Uri> configureUri)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configureUri);

            return builder.ConfigureHttpClient((serviceProvider, client) => client.BaseAddress = configureUri(serviceProvider));
        }

        #endregion

        #region RedactLoggedHeaders

        /// <summary>
        /// Sets the default delegate which determines whether to redact the HTTP header value before logging.
        /// </summary>
        /// <param name="shouldRedactHeaderValue">The delegate which determines whether to redact the HTTP header value before logging.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// The provided <paramref name="shouldRedactHeaderValue"/> predicate will be evaluated for each header value when logging. If the predicate returns <c>true</c> then the header value will be replaced with a marker value <c>*</c> in logs; otherwise the header value will be logged.
        /// </para>
        /// <para>
        /// Any value set for a specific API via an <see cref="IHttpClientBuilder"/> overrides this default value.
        /// </para>
        /// </remarks>
        public IApiBuilder RedactLoggedHeaders(Func<string, bool> shouldRedactHeaderValue)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(shouldRedactHeaderValue);

            builder.Services.Configure<ApiFactoryOptions>(options =>
            {
                options.ShouldRedactHeaderValue = shouldRedactHeaderValue;
            });

            return builder;
        }

        /// <summary>
        /// Sets the default collection of HTTP headers names for which values should be redacted before logging.
        /// </summary>
        /// <param name="redactedLoggedHeaderNames">The collection of HTTP headers names for which values should be redacted before logging.</param>
        /// <returns>The <see cref="IApiBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// Any value set for a specific API via an <see cref="IHttpClientBuilder"/> overrides this default value.
        /// </para>
        /// </remarks>
        public IApiBuilder RedactLoggedHeaders(IEnumerable<string> redactedLoggedHeaderNames)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(redactedLoggedHeaderNames);

            builder.Services.Configure<ApiFactoryOptions>(options =>
            {
                var sensitiveHeaders = new HashSet<string>(redactedLoggedHeaderNames, StringComparer.OrdinalIgnoreCase);

                options.ShouldRedactHeaderValue = (header) => sensitiveHeaders.Contains(header);
            });

            return builder;
        }

        #endregion

        #region SetHandlerLifetime

        /// <summary>
        /// Apply a default length of time that a <see cref="HttpMessageHandler"/> instance can be reused to all APIs.
        /// The default value is two minutes. Set the lifetime to <see cref="Timeout.InfiniteTimeSpan"/> to disable handler expiry.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Any value set for a specific API via an <see cref="IHttpClientBuilder"/> overrides this default value.
        /// </para>
        /// <para>
        /// See <see cref="HttpClientBuilderExtensions.SetHandlerLifetime"/> for more details.
        /// </para>
        /// </remarks>
        public IApiBuilder SetHandlerLifetime(TimeSpan handlerLifetime)
        {
            ArgumentNullException.ThrowIfNull(builder);

            if (handlerLifetime != Timeout.InfiniteTimeSpan && handlerLifetime < s_minimumHandlerLifetime)
            {
                throw new ArgumentException("Invalid value.", nameof(handlerLifetime));
            }

            builder.Services.Configure<ApiFactoryOptions>(options => options.HandlerLifetime = handlerLifetime);
            return builder;
        }

        #endregion
    }

    #region Helpers

    private static void AddCommonConfiguration(IHttpClientBuilder builder)
    {
        builder.Services.AddSingleton<IConfigureOptions<HttpClientFactoryOptions>>(serviceProvider =>
            ActivatorUtilities.CreateInstance<YardarmConfigureHttpClientFactoryOptions>(serviceProvider,
                builder.Name));
    }

    [DoesNotReturn]
    private static void ThrowRegistrationError(Type clientType)
    {
        throw new InvalidOperationException(
            $"The API {clientType} has already been registered. It may only be registered once.");
    }

    #endregion
}
