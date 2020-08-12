using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Features;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.Enrichment.Responses.Internal
{
    internal class BaseTypeEnricher : IResponseClassEnricher
    {
        private readonly GenerationContext _context;

        public int Priority => 0;

        public BaseTypeEnricher(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            LocatedOpenApiElement<OpenApiResponse> context)
        {
            var feature = _context.Features.Get<IResponseBaseTypeFeature>();
            if (feature == null)
            {
                return target;
            }

            BaseTypeSyntax[] additionalBaseTypes = feature.GetBaseTypes(context).ToArray();

            return additionalBaseTypes.Length > 0
                ? target.AddBaseListTypes(additionalBaseTypes)
                : target;
        }
    }
}
