using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Schema
{
    public interface ISchemaBaseTypeRegistry
    {
        void AddBaseType(ILocatedOpenApiElement<IOpenApiSchema> schema, BaseTypeSyntax type);

        IEnumerable<BaseTypeSyntax> GetBaseTypes(ILocatedOpenApiElement<IOpenApiSchema> schema);
    }
}
