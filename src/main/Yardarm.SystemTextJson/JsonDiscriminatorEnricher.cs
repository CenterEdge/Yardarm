using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.SystemTextJson.Helpers;
using Yardarm.SystemTextJson.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson;

public class JsonDiscriminatorEnricher : IOpenApiSyntaxNodeEnricher<InterfaceDeclarationSyntax, OpenApiSchema>,
    IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiSchema>
{
    protected GenerationContext GenerationContext { get; }
    protected ITypeGeneratorRegistry<OpenApiSchema> TypeGeneratorRegistry { get; }

    public JsonDiscriminatorEnricher(GenerationContext generationContext,
        [FromKeyedServices(DiscriminatorConverterTypeGenerator.GeneratorCategory)] ITypeGeneratorRegistry<OpenApiSchema> typeGeneratorRegistry)
    {
        ArgumentNullException.ThrowIfNull(generationContext);
        ArgumentNullException.ThrowIfNull(typeGeneratorRegistry);

        GenerationContext = generationContext;
        TypeGeneratorRegistry = typeGeneratorRegistry;
    }

    public InterfaceDeclarationSyntax Enrich(InterfaceDeclarationSyntax target,
        OpenApiEnrichmentContext<OpenApiSchema> context) =>
        SchemaHelper.IsPolymorphic(context.Element) && context.LocatedElement.IsJsonSchema
            ? (InterfaceDeclarationSyntax) AddJsonConverter(target, context)
            : target;

    public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
        OpenApiEnrichmentContext<OpenApiSchema> context) =>
        SchemaHelper.IsPolymorphic(context.Element) && context.LocatedElement.IsJsonSchema
            ? (ClassDeclarationSyntax) AddJsonConverter(target, context)
            : target;

    protected virtual TypeDeclarationSyntax AddJsonConverter(TypeDeclarationSyntax target,
        OpenApiEnrichmentContext<OpenApiSchema> context)
    {
        var converter = TypeGeneratorRegistry.Get(context.LocatedElement);

        var attributes = new List<AttributeListSyntax>
        {
            AttributeList(SingletonSeparatedList(Attribute(SystemTextJsonTypes.Serialization.JsonConverterAttributeName,
                AttributeArgumentList(
                    SingletonSeparatedList(AttributeArgument(TypeOfExpression(converter.TypeInfo.Name)))))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed),
            AttributeList(SingletonSeparatedList(Attribute(SystemTextJsonTypes.Serialization.JsonPolymorphicAttributeName)))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed)
        };

        // Add JsonDerivedType attributes. This ensures that anytime this type is included on a JsonSerializerContext
        // all of the derived types are also included. This, in turn, allows the JsonConverter to serialize the
        // derived types.
        foreach (var (_, derivedType) in SchemaHelper.GetDiscriminatorMappings(GenerationContext, context.LocatedElement))
        {
            attributes.Add(AttributeList(SingletonSeparatedList(
                    Attribute(SystemTextJsonTypes.Serialization.JsonDerivedTypeAttributeName,
                        AttributeArgumentList(SeparatedList(
                        [
                            AttributeArgument(TypeOfExpression(derivedType))
                        ])))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
        }

        return target.AddAttributeLists([.. attributes]);
    }
}
