using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Helpers;
using Yardarm.Spec;

namespace Yardarm.Features
{
    public class ResponseBaseTypeFeature : IResponseBaseTypeFeature
    {
        private readonly ConcurrentDictionary<LocatedOpenApiElement<OpenApiResponse>, ConcurrentBag<BaseTypeSyntax>> _inheritance =
            new ConcurrentDictionary<LocatedOpenApiElement<OpenApiResponse>, ConcurrentBag<BaseTypeSyntax>>(new LocatedElementEqualityComparer<OpenApiResponse>());

        public void AddBaseType(LocatedOpenApiElement<OpenApiResponse> schema, BaseTypeSyntax type)
        {
            var bag = _inheritance.GetOrAdd(schema, _ => new ConcurrentBag<BaseTypeSyntax>());

            bag.Add(type);
        }

        public IEnumerable<BaseTypeSyntax> GetBaseTypes(LocatedOpenApiElement<OpenApiResponse> schema)
        {
            if (!_inheritance.TryGetValue(schema, out var list))
            {
                return Enumerable.Empty<BaseTypeSyntax>();
            }

            return list;
        }
    }
}
