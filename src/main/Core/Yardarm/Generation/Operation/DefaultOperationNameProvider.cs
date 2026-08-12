using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Operation
{
    internal class DefaultOperationNameProvider : IOperationNameProvider
    {
        public string? GetOperationName(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.Element.OperationId;
    }
}
