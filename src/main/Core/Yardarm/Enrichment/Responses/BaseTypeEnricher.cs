using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;

namespace Yardarm.Enrichment.Responses
{
    public class BaseTypeEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, IOpenApiResponse>
    {
        private readonly IResponseBaseTypeRegistry _responseBaseTypeRegistry;

        public BaseTypeEnricher(IResponseBaseTypeRegistry responseBaseTypeRegistry)
        {
            ArgumentNullException.ThrowIfNull(responseBaseTypeRegistry);

            _responseBaseTypeRegistry = responseBaseTypeRegistry;
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<IOpenApiResponse> context)
        {
            BaseTypeSyntax[] additionalBaseTypes = _responseBaseTypeRegistry
                .GetBaseTypes(context.LocatedElement).ToArray();

            return additionalBaseTypes.Length > 0
                ? target.AddBaseListTypes(additionalBaseTypes)
                : target;
        }
    }
}
