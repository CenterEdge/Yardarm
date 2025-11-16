using Microsoft.Extensions.DependencyInjection;

namespace RootNamespace.Internal;

/// <summary>
/// Default implementation of <see cref="IApiBuilder"/>.
/// </summary>
/// <remarks>
/// Create a new ApiBuilder.
/// </remarks>
/// <param name="services">The application service collection.</param>
internal class ApiBuilder(IServiceCollection services) : IApiBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; set; } = services;
}
