using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.SystemTextJson.Helpers;

namespace Yardarm.SystemTextJson
{
    public class JsonPropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiSchema>
    {
        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            if (target.Parent is ClassDeclarationSyntax classDeclaration &&
                classDeclaration.GetGeneratorAnnotation() == typeof(RequestMediaTypeGenerator))
            {
                // Don't enrich body properties on the request classes
                return target;
            }

            return target.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                SyntaxFactory.Attribute(SystemTextJsonTypes.JsonPropertyNameAttributeName).AddArgumentListArguments(
                    SyntaxFactory.AttributeArgument(SyntaxHelpers.StringLiteral(context.LocatedElement.Key)))));
        }
    }
}
