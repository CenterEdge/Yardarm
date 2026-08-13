using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Operation
{
    public interface IOperationMethodGenerator
    {
        BlockSyntax Generate(ILocatedOpenApiElement<OpenApiOperation> operation);
    }
}
