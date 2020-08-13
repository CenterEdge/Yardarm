using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Schema
{
    public interface ISchemaBaseTypeRegistry
    {
        void AddBaseType(LocatedOpenApiElement<OpenApiSchema> schema, BaseTypeSyntax type);

        IEnumerable<BaseTypeSyntax> GetBaseTypes(LocatedOpenApiElement<OpenApiSchema> schema);
    }
}
