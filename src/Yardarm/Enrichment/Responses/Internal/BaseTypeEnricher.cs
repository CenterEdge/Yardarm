using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Yardarm.Enrichment.Responses.Internal
{
    internal class BaseTypeEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiResponse>
    {
        private readonly GenerationContext _context;

        public int Priority => 0;

        public BaseTypeEnricher(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiResponse> context)
        {
            var feature = _context.GenerationServices.GetRequiredService<IResponseBaseTypeRegistry>();
            if (feature == null)
            {
                return target;
            }

            BaseTypeSyntax[] additionalBaseTypes = feature.GetBaseTypes(context.LocatedElement).ToArray();

            return additionalBaseTypes.Length > 0
                ? target.AddBaseListTypes(additionalBaseTypes)
                : target;
        }
    }
}
