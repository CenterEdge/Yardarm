using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RootNamespace
{
    /// <summary>
    /// A builder for configuring APIs registered with <see cref="IHttpClientFactory"/> and <see cref="IServiceCollection"/>.
    /// </summary>
    public interface IApiBuilder
    {
        /// <summary>
        /// Gets the application service collection.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
