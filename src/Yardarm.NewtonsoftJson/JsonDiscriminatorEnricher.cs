using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment.Schema;
using Yardarm.Generation;
using Yardarm.Helpers;
using Yardarm.NewtonsoftJson.Helpers;

namespace Yardarm.NewtonsoftJson
{
    public class JsonDiscriminatorEnricher : ISchemaInterfaceEnricher
    {
        protected GenerationContext Context { get; }

        public int Priority => 0;

        public JsonDiscriminatorEnricher(GenerationContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public InterfaceDeclarationSyntax Enrich(InterfaceDeclarationSyntax target,
            LocatedOpenApiElement<OpenApiSchema> context) =>
            context.Element.Discriminator?.PropertyName != null
                ? AddJsonConverter(target, context)
                : target;

        protected virtual InterfaceDeclarationSyntax AddJsonConverter(InterfaceDeclarationSyntax target,
            LocatedOpenApiElement<OpenApiSchema> element)
        {
            OpenApiSchema schema = element.Element;

            var attribute = SyntaxFactory.Attribute(JsonHelpers.JsonConverterAttributeName()).AddArgumentListArguments(
                SyntaxFactory.AttributeArgument(
                    SyntaxFactory.TypeOfExpression(JsonHelpers.DiscriminatorConverterName(Context.NamespaceProvider))),
                SyntaxFactory.AttributeArgument(
                    SyntaxHelpers.StringLiteral(schema.Discriminator.PropertyName)),
                SyntaxFactory.AttributeArgument(
                    SyntaxFactory.TypeOfExpression(Context.TypeNameProvider.GetName(element))));

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

                                    OpenApiSchema referencedSchema = schema.OneOf
                                        .FirstOrDefault(p => p.Reference?.ReferenceV3 == mapping.Value);

                                    return referencedSchema != null
                                        ? new ExpressionSyntax[]
                                        {
                                            SyntaxHelpers.StringLiteral(mapping.Key), SyntaxFactory.TypeOfExpression(
                                                Context.TypeNameProvider.GetName(
                                                    referencedSchema.CreateRoot(referencedSchema.Reference.Id)))
                                        }
                                        : Enumerable.Empty<ExpressionSyntax>();
                                }))));

                attribute = attribute.AddArgumentListArguments(SyntaxFactory.AttributeArgument(paramArray));
            }

            return target.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(attribute));
        }
    }
}
