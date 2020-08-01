using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Names
{
    public interface INamespaceProvider
    {
        NameSyntax GetSchemaNamespace(LocatedOpenApiElement<OpenApiSchema> schema);
        NameSyntax GetRequestNamespace(LocatedOpenApiElement<OpenApiRequestBody> requestBody);
    }
}
