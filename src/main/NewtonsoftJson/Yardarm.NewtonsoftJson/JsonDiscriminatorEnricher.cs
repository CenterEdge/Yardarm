using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Enrichment;
using Yardarm.Helpers;
using Yardarm.NewtonsoftJson.Helpers;
using Yardarm.Spec;

namespace Yardarm.NewtonsoftJson
{
    public class JsonDiscriminatorEnricher : IOpenApiSyntaxNodeEnricher<InterfaceDeclarationSyntax, IOpenApiSchema>
    {
        protected GenerationContext Context { get; }
        protected IJsonSerializationNamespace JsonSerializationNamespace { get; }

        public JsonDiscriminatorEnricher(GenerationContext context,
            IJsonSerializationNamespace jsonSerializationNamespace)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(jsonSerializationNamespace);

            Context = context;
            JsonSerializationNamespace = jsonSerializationNamespace;
        }

        public InterfaceDeclarationSyntax Enrich(InterfaceDeclarationSyntax target,
            OpenApiEnrichmentContext<IOpenApiSchema> context) =>
            context.Element.Discriminator?.PropertyName != null
                ? AddJsonConverter(target, context)
                : target;

        protected virtual InterfaceDeclarationSyntax AddJsonConverter(InterfaceDeclarationSyntax target,
            OpenApiEnrichmentContext<IOpenApiSchema> context)
        {
            IOpenApiSchema schema = context.Element;
            var discriminator = schema.Discriminator;

            var attribute = SyntaxFactory.Attribute(NewtonsoftJsonTypes.JsonConverterAttributeName).AddArgumentListArguments(
                SyntaxFactory.AttributeArgument(
                    SyntaxFactory.TypeOfExpression(JsonSerializationNamespace.DiscriminatorConverter)),
                SyntaxFactory.AttributeArgument(
                    SyntaxHelpers.StringLiteral(discriminator?.PropertyName ?? string.Empty)),
                SyntaxFactory.AttributeArgument(
                    SyntaxFactory.TypeOfExpression(Context.TypeGeneratorRegistry.Get(context.LocatedElement).TypeInfo.Name)));

            if (discriminator?.Mapping != null)
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
                            discriminator.Mapping
                                .SelectMany(mapping =>
                                {
                                    // Add two parameters to the object array for each mapping
                                    // First is the string key of the mapping, second is the Type to deserialize

                                    // mapping.Value is now an OpenApiSchemaReference
                                    var mappingReferenceId = mapping.Value.GetReferenceId();
                                    IOpenApiSchema? referencedSchema = schema.OneOf?
                                        .FirstOrDefault(p => p is IOpenApiReferenceHolder && p.GetReferenceId() == mappingReferenceId);

                                    return referencedSchema != null
                                        ? new ExpressionSyntax[]
                                        {
                                            SyntaxHelpers.StringLiteral(mapping.Key), SyntaxFactory.TypeOfExpression(
                                                Context.TypeGeneratorRegistry.Get(
                                                    referencedSchema.CreateRoot(referencedSchema.GetReferenceId()!)).TypeInfo.Name)
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
