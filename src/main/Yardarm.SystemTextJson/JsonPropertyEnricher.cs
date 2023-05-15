using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.SystemTextJson.Helpers;
using Yardarm.SystemTextJson.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson
{
    public class JsonPropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiSchema>
    {
        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            if (!context.LocatedElement.IsJsonSchema())
            {
                // Don't enrich non-JSON schemas
                return target;
            }

            if (target.Parent is ClassDeclarationSyntax classDeclaration &&
                classDeclaration.GetGeneratorAnnotation() == typeof(RequestMediaTypeGenerator))
            {
                // Don't enrich body properties on the request classes
                return target;
            }

            return target.AddAttributeLists(AttributeList(SingletonSeparatedList(
                    Attribute(
                        SystemTextJsonTypes.Serialization.JsonPropertyNameAttributeName,
                        AttributeArgumentList(SingletonSeparatedList(
                        AttributeArgument(SyntaxHelpers.StringLiteral(context.LocatedElement.Key)))))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
        }
    }
}
