using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Schema.Internal
{
    internal class SchemaBaseTypeRegistry : ISchemaBaseTypeRegistry
    {
        private readonly ConcurrentDictionary<ILocatedOpenApiElement<IOpenApiSchema>, ConcurrentDictionary<string, BaseTypeSyntax>> _inheritance =
            new(new LocatedElementEqualityComparer<IOpenApiSchema>());

        public void AddBaseType(ILocatedOpenApiElement<IOpenApiSchema> schema, BaseTypeSyntax type)
        {
            var bag = _inheritance.GetOrAdd(schema,
                _ => new ConcurrentDictionary<string, BaseTypeSyntax>());

            bag.TryAdd(type.ToString(), type);
        }

        public IEnumerable<BaseTypeSyntax> GetBaseTypes(ILocatedOpenApiElement<IOpenApiSchema> schema)
        {
            if (!_inheritance.TryGetValue(schema, out var list))
            {
                return Enumerable.Empty<BaseTypeSyntax>();
            }

            return list.Values;
        }
    }
}
