using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RootNamespace.Internal;
using RootNamespace.Serialization;

namespace RootNamespace;

/// <summary>
/// API extensions for <see cref="IServiceCollection"/>.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Add and configure APIs to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <returns>An <see cref="IApiBuilder"/> to add and configure specific APIs.</returns>
        public IApiBuilder AddApis()
        {
            services.TryAddSingleton(static serviceProvider =>
            {
                var apiFactoryOptions = serviceProvider.GetRequiredService<IOptions<ApiFactoryOptions>>().Value;

                var authenticators = new Authentication.Authenticators();
                foreach (var action in apiFactoryOptions.AuthenticatorActions)
                {
                    action(authenticators);
                }

                return authenticators;
            });

            services.TryAddSingleton(static _ => TypeSerializerRegistry.Instance); // Use a delegate to lazy initialize the instance

            return new ApiBuilder(services);
        }
    }
}
