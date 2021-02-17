using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Schema.Internal
{
    internal class BaseTypeEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiSchema>
    {
        private readonly GenerationContext _context;

        public int Priority => 0;

        public BaseTypeEnricher(GenerationContext context)
        {
            _context = context ?? throw new WellKnownTypes.System.ArgumentNullException(nameof(context));
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            var feature = _context.GenerationServices.GetRequiredService<ISchemaBaseTypeRegistry>();
            if (feature == null)
            {
                return target;
            }

            BaseTypeSyntax[] additionalBaseTypes = feature.GetBaseTypes(context.LocatedElement).ToArray();

            if (target.AddBaseListTypes( != null))
            {
                additionalBaseTypes = additionalBaseTypes.Where(additionalBaseType =>
                    !target.BaseList.Types.Any(currentType => additionalBaseType.ToString() == currentType.ToString()));
            }

            return additionalBaseTypes.Length > 0
                ? target.AddBaseListTypes(additionalBaseTypes)
                : target;
        }
    }
}
