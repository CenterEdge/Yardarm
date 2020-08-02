using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Names
{
    public interface INamespaceProvider
    {
        NameSyntax GetRootNamespace();
        NameSyntax GetRequestBodyNamespace(LocatedOpenApiElement<OpenApiRequestBody> requestBody);
        NameSyntax GetResponseNamespace(LocatedOpenApiElement<OpenApiResponse> response);
        NameSyntax GetSchemaNamespace(LocatedOpenApiElement<OpenApiSchema> schema);
    }
}
