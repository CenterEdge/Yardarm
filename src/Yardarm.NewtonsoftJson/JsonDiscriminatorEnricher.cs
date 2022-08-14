using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Helpers;
using Yardarm.NewtonsoftJson.Helpers;
using Yardarm.Spec;

namespace Yardarm.NewtonsoftJson
{
    public class JsonDiscriminatorEnricher : IOpenApiSyntaxNodeEnricher<InterfaceDeclarationSyntax, OpenApiSchema>
    {
        protected GenerationContext Context { get; }
        protected IJsonSerializationNamespace JsonSerializationNamespace { get; }

        public JsonDiscriminatorEnricher(GenerationContext context,
            IJsonSerializationNamespace jsonSerializationNamespace)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            JsonSerializationNamespace = jsonSerializationNamespace ?? throw new ArgumentNullException(nameof(jsonSerializationNamespace));
        }

        public InterfaceDeclarationSyntax Enrich(InterfaceDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context) =>
            context.Element.Discriminator?.PropertyName != null
                ? AddJsonConverter(target, context)
                : target;

        protected virtual InterfaceDeclarationSyntax AddJsonConverter(InterfaceDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            OpenApiSchema schema = context.Element;

            var attribute = SyntaxFactory.Attribute(NewtonsoftJsonTypes.JsonConverterAttributeName).AddArgumentListArguments(
                SyntaxFactory.AttributeArgument(
                    SyntaxFactory.TypeOfExpression(JsonSerializationNamespace.DiscriminatorConverter)),
                SyntaxFactory.AttributeArgument(
                    SyntaxHelpers.StringLiteral(schema.Discriminator.PropertyName)),
                SyntaxFactory.AttributeArgument(
                    SyntaxFactory.TypeOfExpression(Context.TypeGeneratorRegistry.Get(context.LocatedElement).TypeInfo.Name)));

            if (schema.Discriminator.Mapping != null)
            {
                var paramArray = SyntaxFactory.ArrayCreationExpression(
                        SyntaxFactory
                            .ArrayType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                            .WithRankSpecifiers(
                                SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(
                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                        SyntaxFactory.OmittedArraySizeExpression())))))
                    .WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(
                            schema.Discriminator.Mapping
                                .SelectMany(mapping =>
                                {
                                    // Add two parameters to the object array for each mapping
                                    // First is the string key of the mapping, second is the Type to deserialize

                                    OpenApiSchema? referencedSchema = schema.OneOf
                                        .FirstOrDefault(p => p.Reference?.ReferenceV3 == mapping.Value);

                                    return referencedSchema != null
                                        ? new ExpressionSyntax[]
                                        {
                                            SyntaxHelpers.StringLiteral(mapping.Key), SyntaxFactory.TypeOfExpression(
                                                Context.TypeGeneratorRegistry.Get(
                                                    referencedSchema.CreateRoot(referencedSchema.Reference.Id)).TypeInfo.Name)
                                        }
                                        : Enumerable.Empty<ExpressionSyntax>();
                                }))));

                attribute = attribute.AddArgumentListArguments(SyntaxFactory.AttributeArgument(paramArray));
            }

            return target.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(attribute)
                .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));
        }
    }
}
