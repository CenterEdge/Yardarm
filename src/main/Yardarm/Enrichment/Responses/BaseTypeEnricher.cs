using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace Yardarm.Enrichment.Responses
{
    public class BaseTypeEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiResponse>
    {
        private readonly IResponseBaseTypeRegistry _responseBaseTypeRegistry;

        public BaseTypeEnricher(IResponseBaseTypeRegistry responseBaseTypeRegistry)
        {
            ArgumentNullException.ThrowIfNull(responseBaseTypeRegistry);

            _responseBaseTypeRegistry = responseBaseTypeRegistry;
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiResponse> context)
        {
            BaseTypeSyntax[] additionalBaseTypes = _responseBaseTypeRegistry
                .GetBaseTypes(context.LocatedElement).ToArray();

            return additionalBaseTypes.Length > 0
                ? target.AddBaseListTypes(additionalBaseTypes)
                : target;
        }
    }
}
