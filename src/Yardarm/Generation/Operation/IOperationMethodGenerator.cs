using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Operation
{
    public interface IOperationMethodGenerator
    {
        BlockSyntax Generate(LocatedOpenApiElement<OpenApiOperation> operation);
    }
}
