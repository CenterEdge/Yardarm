using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.NewtonsoftJson.Helpers;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NewtonsoftJson
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

            var attribute = Attribute(NewtonsoftJsonTypes.JsonPropertyAttributeName,
                AttributeArgumentList(SingletonSeparatedList(
                    AttributeArgument(SyntaxHelpers.StringLiteral(context.LocatedElement.Key)))));

            bool isRequired =
                context.LocatedElement.Parent is LocatedOpenApiElement<OpenApiSchema> parentSchema &&
                parentSchema.Element.Required.Contains(context.LocatedElement.Key);

            bool isNullable = context.LocatedElement.Element.Nullable;

            if (!isRequired && !isNullable)
            {
                // We prefer not to send null values if the property is not required.
                // However, for nullable properties, prefer to send the null explicitly.
                // This is a compromise due to .NET not supporting a concept of null vs missing.

                attribute = attribute.AddArgumentListArguments(AttributeArgument(
                    NameEquals(IdentifierName("NullValueHandling")),
                    null,
                    NewtonsoftJsonTypes.NullValueHandling.Ignore));
            }

            return target.AddAttributeLists(AttributeList(SingletonSeparatedList(attribute))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
        }
    }
}
