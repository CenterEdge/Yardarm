using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.SystemTextJson.Helpers;
using Yardarm.SystemTextJson.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson
{
    public class JsonDiscriminatorEnricher : IOpenApiSyntaxNodeEnricher<InterfaceDeclarationSyntax, OpenApiSchema>,
        IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiSchema>
    {
        protected GenerationContext GenerationContext { get; }
        protected ITypeGeneratorRegistry<OpenApiSchema, SystemTextJsonGeneratorCategory> TypeGeneratorRegistry { get; }

        public JsonDiscriminatorEnricher(GenerationContext generationContext,
            ITypeGeneratorRegistry<OpenApiSchema, SystemTextJsonGeneratorCategory> typeGeneratorRegistry)
        {
            GenerationContext = generationContext ?? throw new ArgumentNullException(nameof(generationContext));
            TypeGeneratorRegistry = typeGeneratorRegistry ?? throw new ArgumentNullException(nameof(typeGeneratorRegistry));
        }

        public InterfaceDeclarationSyntax Enrich(InterfaceDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context) =>
            context.Element.Discriminator?.PropertyName is not null && context.LocatedElement.IsJsonSchema()
                ? (InterfaceDeclarationSyntax) AddJsonConverter(target, context)
                : target;

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context) =>
            context.Element.Discriminator?.PropertyName is not null && context.LocatedElement.IsJsonSchema()
                ? (ClassDeclarationSyntax) AddJsonConverter(target, context)
                : target;

        protected virtual TypeDeclarationSyntax AddJsonConverter(TypeDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            var converter = TypeGeneratorRegistry.Get(context.LocatedElement);

            var attribute = Attribute(SystemTextJsonTypes.Serialization.JsonConverterAttributeName,
                AttributeArgumentList(SingletonSeparatedList(AttributeArgument(TypeOfExpression(converter.TypeInfo.Name)))));

            return target.AddAttributeLists(AttributeList(SingletonSeparatedList(attribute))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
        }
    }
}
