using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Spec;
using Yardarm.SystemTextJson.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson
{
    public class JsonDateOnlyPropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiSchema>
    {
        private readonly IOpenApiElementRegistry _elementRegistry;
        private readonly IJsonSerializationNamespace _serializationNamespace;

        public JsonDateOnlyPropertyEnricher(IOpenApiElementRegistry elementRegistry, IJsonSerializationNamespace serializationNamespace)
        {
            ArgumentNullException.ThrowIfNull(elementRegistry);
            ArgumentNullException.ThrowIfNull(serializationNamespace);

            _elementRegistry = elementRegistry;
            _serializationNamespace = serializationNamespace;
        }

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax syntax, OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            if (context.Element.Type != "string" || context.Element.Format is not "date" and not "full-date")
            {
                // Only applies to date-only strings
                return syntax;
            }

            if (syntax.Parent?.GetElementAnnotation<OpenApiSchema>(_elementRegistry) is null)
            {
                // We don't need to apply this to properties of request classes, only schemas
                return syntax;
            }

            var model = context.Compilation.GetSemanticModel(context.SyntaxTree);
            TypeInfo typeInfo = model.GetTypeInfo(syntax.Type);
            if (!IsDateTime(typeInfo.Type as INamedTypeSymbol))
            {
                // Don't apply if some other process has changed the type to something other than System.DateTime
                return syntax;
            }

            return AddJsonConverterAttribute(syntax);
        }

        private PropertyDeclarationSyntax AddJsonConverterAttribute(PropertyDeclarationSyntax syntax) =>
            syntax
                .AddAttributeLists(AttributeList(SingletonSeparatedList(
                    Attribute(SystemTextJsonTypes.Serialization.JsonConverterAttributeName,
                        AttributeArgumentList(SingletonSeparatedList(AttributeArgument(
                            SyntaxFactory.TypeOfExpression(_serializationNamespace.JsonDateConverter))))))))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed);

        private static bool IsDateTime(INamedTypeSymbol? type)
        {
            if (type is null)
            {
                return false;
            }

            if (type.IsGenericType && type.ContainingNamespace.Name == "System" && type.Name == "Nullable")
            {
                if (type.TypeArguments.Length == 0)
                {
                    return false;
                }

                return IsDateTime(type.TypeArguments[0] as INamedTypeSymbol);
            }

            return !type.IsGenericType && type.ContainingNamespace.Name == "System" && type.Name == "DateTime";
        }
    }
}
