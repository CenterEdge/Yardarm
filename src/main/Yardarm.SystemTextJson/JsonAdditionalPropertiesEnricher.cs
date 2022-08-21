using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Schema;
using Yardarm.Generation;
using Yardarm.Helpers;
using Yardarm.SystemTextJson.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson
{
    /// <summary>
    /// Updates additional properties members with the <see cref="JsonExtensionDataAttribute" />.
    /// </summary>
    public class JsonAdditionalPropertiesEnricher : IOpenApiSyntaxNodeEnricher<CompilationUnitSyntax, OpenApiSchema>
    {
        public Type[] ExecuteAfter { get; } =
        {
            typeof(AdditionalPropertiesEnricher)
        };

        public CompilationUnitSyntax Enrich(CompilationUnitSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            var members = target.GetSpecialMembers(SpecialMembers.AdditionalProperties)
                .OfType<PropertyDeclarationSyntax>().ToArray();

            if (members.Length == 0)
            {
                return target;
            }

            target = target.TrackNodes((IEnumerable<PropertyDeclarationSyntax>) members);

            return members.Aggregate(target,
                (current, member) => current.ReplaceNode(current.GetCurrentNode(member)!, AddAttribute(member)));
        }

        private PropertyDeclarationSyntax AddAttribute(PropertyDeclarationSyntax property)
        {
            var dictionaryType = property.Type;

            if (dictionaryType is QualifiedNameSyntax qualifiedName)
            {
                dictionaryType = qualifiedName.Right;
            }

            if (dictionaryType is not GenericNameSyntax genericName)
            {
                // Don't mutate
                return property;
            }

            var nullableObject = NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword)));

            // System.Text.Json requires dictionary values be JsonElement or object, so replace the types with object.
            // We don't want to use JsonElement because it's very difficult to build those dynamically.
            // We could use JsonObject instead of a dictionary, but there is currently an issue in System.Text.Json which prevents this
            // https://github.com/dotnet/runtime/issues/60560
            var newDictionaryType = WellKnownTypes.System.Collections.Generic.DictionaryT.Name(
                genericName.TypeArgumentList.Arguments[0],
                nullableObject);

            var interfaceType = WellKnownTypes.System.Collections.Generic.IDictionaryT.Name(
                genericName.TypeArgumentList.Arguments[0],
                nullableObject);

            return property
                .WithType(interfaceType)
                .WithInitializer(EqualsValueClause(ObjectCreationExpression(newDictionaryType)))
                // We must have a setter for JsonExtensionData to work with System.Text.Json
                .WithAccessorList(AccessorList(property.AccessorList!.Accessors.Add(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration))))
                // Add the JsonExtensionData attribute
                .AddAttributeLists(AttributeList(SingletonSeparatedList(
                Attribute(SystemTextJsonTypes.Serialization.JsonExtensionDataAttributeName)))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
        }
    }
}
