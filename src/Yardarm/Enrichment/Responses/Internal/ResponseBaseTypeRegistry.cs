using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Responses.Internal
{
    internal class ResponseBaseTypeRegistry : IResponseBaseTypeRegistry
    {
        private readonly ConcurrentDictionary<ILocatedOpenApiElement<OpenApiResponse>, ConcurrentBag<BaseTypeSyntax>> _inheritance =
            new ConcurrentDictionary<ILocatedOpenApiElement<OpenApiResponse>, ConcurrentBag<BaseTypeSyntax>>(new LocatedElementEqualityComparer<OpenApiResponse>());

        public void AddBaseType(ILocatedOpenApiElement<OpenApiResponse> schema, BaseTypeSyntax type)
        {
            var bag = _inheritance.GetOrAdd(schema, _ => new ConcurrentBag<BaseTypeSyntax>());

            bag.Add(type);
        }

        public IEnumerable<BaseTypeSyntax> GetBaseTypes(ILocatedOpenApiElement<OpenApiResponse> schema)
        {
            if (!_inheritance.TryGetValue(schema, out var list))
            {
                return Enumerable.Empty<BaseTypeSyntax>();
            }

            return list;
        }
    }
}
