using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Responses
{
    /// <summary>
    /// An operation response with a specific body schema.
    /// </summary>
    /// <typeparam name="TBody">The type of the body schema.</typeparam>
    public interface IOperationResponse<TBody> : IOperationResponse
    {
        /// <summary>
        /// Get the body of the response.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The deserialized body.</returns>
        ValueTask<TBody> GetBodyAsync(CancellationToken cancellationToken = default);
    }
}
