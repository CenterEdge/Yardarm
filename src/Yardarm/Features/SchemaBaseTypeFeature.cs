using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Generation.Schema;

namespace Yardarm.Features
{
    public class SchemaBaseTypeFeature : ISchemaBaseTypeFeature
    {
        private readonly IDictionary<LocatedOpenApiElement<OpenApiSchema>, IList<BaseTypeSyntax>> _inheritance =
            new Dictionary<LocatedOpenApiElement<OpenApiSchema>, IList<BaseTypeSyntax>>(new SchemaEqualityComparer());

        public void AddBaseType(LocatedOpenApiElement<OpenApiSchema> schema, BaseTypeSyntax type)
        {
            if (!_inheritance.TryGetValue(schema, out var list))
            {
                list = new List<BaseTypeSyntax>();
                _inheritance[schema] = list;
            }

            list.Add(type);
        }

        public IEnumerable<BaseTypeSyntax> GetBaseTypes(LocatedOpenApiElement<OpenApiSchema> schema)
        {
            if (!_inheritance.TryGetValue(schema, out var list))
            {
                return Enumerable.Empty<BaseTypeSyntax>();
            }

            return list;
        }
    }
}
