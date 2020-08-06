using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Features
{
    public interface ISchemaBaseTypeFeature
    {
        void AddBaseType(LocatedOpenApiElement<OpenApiSchema> schema, BaseTypeSyntax type);

        IEnumerable<BaseTypeSyntax> GetBaseTypes(LocatedOpenApiElement<OpenApiSchema> schema);
    }
}
