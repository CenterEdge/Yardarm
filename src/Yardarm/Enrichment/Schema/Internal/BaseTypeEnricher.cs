using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Yardarm.Enrichment.Schema.Internal
{
    internal class BaseTypeEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiSchema>
    {
        private readonly GenerationContext _context;

        public int Priority => 0;

        public BaseTypeEnricher(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            var feature = _context.GenerationServices.GetRequiredService<ISchemaBaseTypeRegistry>();
            if (feature == null)
            {
                return target;
            }

            var additionalBaseTypes = feature.GetBaseTypes(context.LocatedElement);

            if (target.BaseList != null)
            {
                additionalBaseTypes = additionalBaseTypes.Where(additionalBaseType =>
                    target.BaseList.Types.Any(currentType => !currentType.IsEquivalentTo(additionalBaseType)));
            }

            var typesToAdd = additionalBaseTypes.ToArray();
            return typesToAdd.Length > 0
                ? target.AddBaseListTypes(typesToAdd)
                : target;
        }
    }
}
