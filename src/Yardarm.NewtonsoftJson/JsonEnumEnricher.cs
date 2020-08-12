using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment.Schema;
using Yardarm.Generation;
using Yardarm.NewtonsoftJson.Helpers;
using Yardarm.Spec;

namespace Yardarm.NewtonsoftJson
{
    public class JsonEnumEnricher : ISchemaEnumEnricher
    {
        public int Priority => 0;

        public EnumDeclarationSyntax Enrich(EnumDeclarationSyntax target,
            LocatedOpenApiElement<OpenApiSchema> context) =>
            context.Element.Type == "string"
                ? target
                    .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                        SyntaxFactory.Attribute(JsonHelpers.JsonConverterAttributeName()).AddArgumentListArguments(
                            SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(JsonHelpers.StringEnumConverterName())))))
                : target;
    }
}
