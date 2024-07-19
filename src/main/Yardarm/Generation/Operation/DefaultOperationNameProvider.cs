using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Operation
{
    internal class DefaultOperationNameProvider : IOperationNameProvider
    {
        public string? GetOperationName(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.Element.OperationId;
    }
}
