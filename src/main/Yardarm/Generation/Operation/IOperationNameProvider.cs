using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Operation
{
    /// <summary>
    /// Provides the name for an operation, used to control the generation of methods and classes
    /// related to the operation.
    /// </summary>
    public interface IOperationNameProvider
    {
        /// <summary>
        /// Provides the name for an operation, used to control the generation of methods and classes
        /// related to the operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <returns>The name of the operation, or null if the operation has no name and should be excluded.</returns>
        string? GetOperationName(ILocatedOpenApiElement<OpenApiOperation> operation);
    }
}
