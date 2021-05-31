using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public interface IRequestMemberGenerator
    {
        MemberDeclarationSyntax Generate(ILocatedOpenApiElement<OpenApiOperation> operation, ILocatedOpenApiElement<OpenApiMediaType>? mediaType);
    }
}
