using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Responses
{
    public interface IResponseBaseTypeRegistry
    {
        void AddBaseType(ILocatedOpenApiElement<IOpenApiResponse> response, BaseTypeSyntax type);

        IEnumerable<BaseTypeSyntax> GetBaseTypes(ILocatedOpenApiElement<IOpenApiResponse> response);
    }
}
