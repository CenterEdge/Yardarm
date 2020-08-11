using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Features
{
    public interface IResponseBaseTypeFeature
    {
        void AddBaseType(LocatedOpenApiElement<OpenApiResponse> schema, BaseTypeSyntax type);

        IEnumerable<BaseTypeSyntax> GetBaseTypes(LocatedOpenApiElement<OpenApiResponse> schema);
    }
}
