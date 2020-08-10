using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Helpers;

namespace Yardarm.Features
{
    public class SchemaBaseTypeFeature : ISchemaBaseTypeFeature
    {
        private readonly ConcurrentDictionary<LocatedOpenApiElement<OpenApiSchema>, ConcurrentBag<BaseTypeSyntax>> _inheritance =
            new ConcurrentDictionary<LocatedOpenApiElement<OpenApiSchema>, ConcurrentBag<BaseTypeSyntax>>(new LocatedElementEqualityComparer<OpenApiSchema>());

        public void AddBaseType(LocatedOpenApiElement<OpenApiSchema> schema, BaseTypeSyntax type)
        {
            var bag = _inheritance.GetOrAdd(schema, _ => new ConcurrentBag<BaseTypeSyntax>());

            bag.Add(type);
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
