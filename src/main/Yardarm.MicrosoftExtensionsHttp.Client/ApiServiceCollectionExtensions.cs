using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RootNamespace.Internal;
using RootNamespace.Serialization;

namespace RootNamespace
{
    /// <summary>
    /// API extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ApiServiceCollectionExtensions
    {
        /// <summary>
        /// Add and configure APIs to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>An <see cref="IApiBuilder"/> to add and configure specific APIs.</returns>
        public static IApiBuilder AddApis(this IServiceCollection services)
        {
            services.TryAddSingleton<Authentication.Authenticators>();
            services.TryAddSingleton(static _ => TypeSerializerRegistry.Instance); // Use a delegate to lazy initialize the instance
            services.TryAddSingleton<ApiFactory>();

            return new ApiBuilder(services);
        }
    }
}
