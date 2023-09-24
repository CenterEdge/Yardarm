using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Yardarm.Enrichment.Schema
{
    public class BaseTypeEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiSchema>
    {
        private readonly GenerationContext _context;

        public BaseTypeEnricher(GenerationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _context = context;
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            var feature = _context.GenerationServices.GetRequiredService<ISchemaBaseTypeRegistry>();
            var additionalBaseTypes = feature.GetBaseTypes(context.LocatedElement);

            if (target.BaseList != null)
            {
                additionalBaseTypes = additionalBaseTypes.Where(additionalBaseType =>
                    target.BaseList.Types.Any(currentType => !currentType.IsEquivalentTo(additionalBaseType)));
            }

            var arr = additionalBaseTypes.ToArray();
            return arr.Length > 0
                ? target.AddBaseListTypes(arr)
                : target;
        }
    }
}
