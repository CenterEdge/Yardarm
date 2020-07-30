using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace Yardarm.Names
{
    public interface INamespaceProvider
    {
        NameSyntax GetSchemaNamespace(OpenApiSchema schema);
    }
}
