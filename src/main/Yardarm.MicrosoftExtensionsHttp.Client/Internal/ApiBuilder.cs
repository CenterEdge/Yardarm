using Microsoft.Extensions.DependencyInjection;

namespace RootNamespace.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IApiBuilder"/>.
    /// </summary>
    internal class ApiBuilder : IApiBuilder
    {
        /// <inheritdoc />
        public IServiceCollection Services { get; set; }

        /// <summary>
        /// Create a new ApiBuilder.
        /// </summary>
        /// <param name="services">The application service collection.</param>
        public ApiBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
