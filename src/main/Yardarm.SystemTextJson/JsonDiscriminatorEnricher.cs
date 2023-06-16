using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.SystemTextJson.Helpers;
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

        internal static bool IsPolymorphic(OpenApiSchema schema) =>
            schema is {Discriminator.PropertyName: not null} or {OneOf.Count: > 0};

        public InterfaceDeclarationSyntax Enrich(InterfaceDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context) =>
            IsPolymorphic(context.Element)
                ? (InterfaceDeclarationSyntax) AddJsonConverter(target, context)
                : target;

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context) =>
            IsPolymorphic(context.Element)
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
